using System.Reflection;
using System.Runtime.CompilerServices;
using AutoBogus.Errors;
using AutoBogus.Internal;
using AutoBogus.Util;
using Bogus.Platform;
using Binder = Bogus.Binder;

namespace AutoBogus;

/// <summary>
/// A default extensible implementation of the <see cref="IAutoBinder"/>.
/// </summary>
public class AutoBinder : Binder, IAutoBinder
{
    /// <summary>
    /// Creates an unpopulated instance of the <typeparamref name="TType"/> type.
    /// The implementation will find an appropriate entry point and create a new instance via it.
    /// Possible entry points are:
    /// <list type="bullet">
    /// <item>
    /// For <c>class</c>es and <c>struct</c>s it is some <c>public</c> constructor
    /// (a parameterless constructor will be chosen if there is any).
    /// If there are no appropriate constructors,
    /// an <see cref="CouldNotFindAppropriateConstructorException"/> will be thrown for the class,
    /// and the <c>default</c> value will be return for the struct.
    /// </item>
    /// <item>
    /// For <c>abstract class</c>es and <c>interface</c>s by default no value will be generated, returning <c>null</c>.
    /// It is a both reasonable and historical AutoBogus default behaviour:
    /// instead of trying to implement its own mocking library,
    /// AutoBogus provides additional packages with bindings to the most popular mocking libraries:
    /// <list type="bullet">
    /// <item><c>AutoBogus.NSubstitue</c></item>
    /// <item><c>AutoBogus.Moq</c></item>
    /// <item><c>AutoBogus.FakeItEasy</c></item>
    /// </list>
    /// Applying one if these, AutoBogus will start to create mocks for interfaces and abstract classes.
    /// Note that code of such package consists of one class direvid from this one with only 20 lines of code:
    /// if there is a need for you to use another library or one of listed above but of the different version,
    /// you may easily provide your own implementation.
    /// </item>
    /// </list>
    /// </summary>
    ///
    /// <typeparam name="TType">The type of the instance being created.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the creation request.</param>
    /// <returns>The created unpopulated instance of the <typeparamref name="TType" /> type.</returns>
    public virtual TType CreateUnpopulatedInstance<TType>(AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type        = typeof(TType);
        var constructor = GetConstructorForNewInstanceCreation<TType>();

        if (constructor == null)
        {
            if (type.IsInterface || type.IsAbstract || type.IsValueType)
            {
                return default!;
            }

            throw new CouldNotFindAppropriateConstructorException(
                $"""
                 No public constructor has been found on the type {type}.
                 AutoFaker must have access to some public constructor of type in order to create an instance of it.
                 """);
        }

        // If a constructor is found generate values for each of the parameters
        var generatedConstructorArguments = constructor
            .GetParameters()
            .Select(it => Generation.Generate(context, type, it.ParameterType, it.Name))
            .ToArray();

        var instance = (TType)constructor.Invoke(generatedConstructorArguments);

        return instance;
    }

    /// <inheritdoc />
    public virtual void PopulateInstance<TType>(
        object?                  instance,
        AutoGenerateContext      context,
        IEnumerable<MemberInfo>? members = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (instance == null)
        {
            return;
        }

        var instanceType = typeof(TType);

        // Iterate the members and bind a generated value
        var membersToPopulate = GetMembersToPopulate(instanceType, members)
            .Select(it => new PopulationTarget(it))
            .ToList();

        foreach (var member in membersToPopulate)
        {
            if (ShouldPopulateMember(instanceType, member.Name, member.Type, context))
            {
                context.TypesStack.Push(member.Type);

                var value = Generation.Generate(context, instanceType, member.Type, member.Name);

                Population.PopulateWithNewValue(member, instance, value);

                context.TypesStack.Pop();
            }
        }
    }

    /// <summary>
    /// Tries to obtains a public constructor of the <typeparamref name="TType"/> type
    /// which is to be used as an entry-point for the instance creation.
    /// </summary>
    /// <typeparam name="TType">The type of the instance being created.</typeparam>
    /// <returns>An appropriate constructor if found, <c>null</c> otherwise.</returns>
    protected virtual ConstructorInfo? GetConstructorForNewInstanceCreation<TType>()
    {
        var type         = typeof(TType);
        var constructors = type.GetConstructors();

        // For dictionaries and enumerables locate a constructor that is used for populating as well
        if (ReflectionHelper.IsDictionary(type))
        {
            return ReflectionHelper.ResolveTypedConstructor(typeof(IDictionary<,>), constructors);
        }

        if (ReflectionHelper.IsEnumerable(type))
        {
            return ReflectionHelper.ResolveTypedConstructor(typeof(IEnumerable<>), constructors);
        }

        // An attempt to find a parameterless constructor.
        // If one is not found the first (as random) one will be used as
        // there is no reasoning why one would be better than other.
        var parameterlessConstructor = constructors.SingleOrDefault(it => it.GetParameters().Length == 0);

        return parameterlessConstructor ?? constructors.FirstOrDefault();
    }

    /// <summary>
    /// Check if the member has a skip config or the type has already been generated as a parent.
    /// If so skip this generation otherwise track it for use later in the object tree.
    /// </summary>
    /// <param name="instanceType">Type of instance which member is to be populated.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="memberType">The type of the member.</param>
    /// <param name="context">The generation context.</param>
    /// <returns>true of <paramref name="memberName"/> should be populated, false otherwise.</returns>
    protected virtual bool ShouldPopulateMember(
        Type                instanceType,
        string              memberName,
        Type                memberType,
        AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!PopulationTargetFiltering.ShoudPopulate(context, instanceType, memberName, memberType))
        {
            return false;
        }

        // check if tree depth is reached
        var treeDepth = context.Config.TreeDepth.Invoke(context);

        if (treeDepth.HasValue && context.TypesStack.Count >= treeDepth)
        {
            return false;
        }

        // Finally check if the recursive depth has been reached
        var count          = context.TypesStack.Count(t => t == memberType);
        var recursiveDepth = context.Config.RecursiveDepth.Invoke(context);

        return count < recursiveDepth;
    }

    /// <summary>
    /// Mostly repeats <see cref="Binder.GetMembers"/> logic but additionaly
    /// allowes <see cref="IDictionary{TKey,TValue}"/> or <see cref="ICollection{T}"/> typed readonly properties,
    /// as they can be filled via their <c>Add(...)</c> methods.
    /// </summary>
    /// <returns>Members of type <paramref name="type"/> which should be populated.</returns>
    protected virtual IEnumerable<MemberInfo> GetMembersToPopulate(
        Type                     type,
        IEnumerable<MemberInfo>? membersSelectedByClient)
    {
        ArgumentNullException.ThrowIfNull(type);

        // If a list of members is provided, no others should be populated
        if (membersSelectedByClient != null)
        {
            return membersSelectedByClient;
        }

        var candidates = type
            .GetAllMembers(BindingFlags)
            .Select(it => UseBaseTypeDeclaredPropertyInfo(type, it))
            .Where(it =>
            {
                // no compiler generated stuff
                if (it.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any())
                {
                    return false;
                }

                return it switch
                {
                    PropertyInfo property =>
                        property.CanWrite ||
                        ReflectionHelper.IsDictionary(property.PropertyType) ||
                        ReflectionHelper.IsCollection(property.PropertyType),

                    FieldInfo field => !field.IsPrivate,

                    _ => false,
                };
            });

        var deduplicatedByName = candidates
            .GroupBy(it => it.Name)
            .ToDictionary(it => it.Key, it => it.First())
            .Values;

        return deduplicatedByName;
    }
}

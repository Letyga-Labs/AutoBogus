using System.Reflection;
using System.Runtime.CompilerServices;
using AutoBogus.Errors;
using AutoBogus.Generation;
using AutoBogus.Util;
using Bogus.Platform;
using Binder = Bogus.Binder;

namespace AutoBogus;

/// <summary>
///     A class for binding generated instances.
/// </summary>
public class AutoBinder : Binder, IAutoBinder
{
    /// <summary>
    ///     Creates an instance of <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The type of instance to create.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the generate request.</param>
    /// <returns>The created instance of <typeparamref name="TType" />.</returns>
    /// <remarks>
    ///     Historically, AutoBogus does not create instances for Interfaces and Abstract classes,
    ///     leaving it with nulls. Additional packages (AutoBogus.NSubstitue, AutoBogus.Moq, AutoBogus.FakeItEasy)
    ///     are provided, which create mocks for such types.
    /// </remarks>
    public virtual TType CreateInstance<TType>(AutoGenerateContext context)
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
            .Select(it => Generator.Generate(context, type, it.ParameterType, it.Name))
            .ToArray();

        var instance = (TType)constructor.Invoke(generatedConstructorArguments);

        return instance;
    }

    /// <summary>
    ///     Populates the provided instance with generated values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to populate.</typeparam>
    /// <param name="instance">The instance to populate.</param>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the generate request.</param>
    /// <param name="members">
    ///     An optional collection of members to populate. If null, all writable instance members are
    ///     populated.
    /// </param>
    /// <remarks>
    ///     Due to the boxing nature of value types, the <paramref name="instance" /> parameter is an object.
    ///     This means the populated values are applied to the provided instance and not a copy.
    /// </remarks>
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

        var type = typeof(TType);

        // Iterate the members and bind a generated value
        var membersToPopulate = GetMembersToPopulate(type, members).Select(it => new AutoMember(it)).ToList();

        foreach (var member in membersToPopulate)
        {
            if (ShouldPopulateMember(type, member.Name, member.Type, context))
            {
                context.TypesStack.Push(member.Type);

                var value = Generator.Generate(context, type, member.Type, member.Name);
                member.PopulateWithNewValue(instance, value);

                context.TypesStack.Pop();
            }
        }
    }

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

        // Attempt to find a default constructor
        // If one is not found, simply use the first in the list
        var defaultConstructor = constructors.SingleOrDefault(it => it.GetParameters().Length == 0);

        return defaultConstructor ?? constructors.FirstOrDefault();
    }

    /// <summary>
    /// Check if the member has a skip config or the type has already been generated as a parent.
    /// If so skip this generation otherwise track it for use later in the object tree.
    /// </summary>
    /// <param name="ownerType">Type of instance which member is to be populated.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="memberType">The type of the member.</param>
    /// <param name="context">The generation context.</param>
    /// <returns>true of <paramref name="memberName"/> should be populated, false otherwise.</returns>
    protected virtual bool ShouldPopulateMember(
        Type                ownerType,
        string              memberName,
        Type                memberType,
        AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (SkipConfig.ShouldSkip(context, ownerType, memberName, memberType))
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
    ///     Mostly repeats <see cref="Binder.GetMembers"/> logic but additionaly
    ///     allowes Dictionary or Collection typed readonly properties, as they can be filled
    ///     via their Add methods.
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

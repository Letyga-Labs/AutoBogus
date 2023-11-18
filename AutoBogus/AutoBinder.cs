using System.Reflection;
using AutoBogus.Errors;
using AutoBogus.Population;
using AutoBogus.Util;
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
    public virtual TType CreateInstance<TType>(AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type        = typeof(TType);
        var constructor = GetConstructorForNewInstanceCreation<TType>();

        if (constructor == null)
        {
            throw new CouldNotFindAppropriateConstructorException(
                $"""
                 No public constructor has been found on type {type}.
                 AutoFaker must have access to some public constructor of type in order to create an instance of it.
                 """);
        }

        // If a constructor is found generate values for each of the parameters
        var generatedConstructorArguments = constructor
            .GetParameters()
            .Select(p => GenerateValue(type, p.ParameterType, p.Name, context))
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
        var membersToPopulate = GetMembersToPopulate(type, members);

        foreach (var member in membersToPopulate)
        {
            if (ShouldPopulateMember(member.Type, member.Name, context))
            {
                context.TypesStack.Push(member.Type);

                var value = GenerateValue(type, member.Type, member.Name, context);
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
    /// <returns>true of <paramref name="memberName"/> should be populated, false otherwise.</returns>
    protected virtual bool ShouldPopulateMember(Type type, string memberName, AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (SkipConfig.ShouldSkip(context, type, memberName))
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
        var count          = context.TypesStack.Count(t => t == type);
        var recursiveDepth = context.Config.RecursiveDepth.Invoke(context);

        return count < recursiveDepth;
    }

    private IEnumerable<AutoMember> GetMembersToPopulate(
        Type                     type,
        IEnumerable<MemberInfo>? methodsSelectedByClient)
    {
        // If a list of members is provided, no others should be populated
        if (methodsSelectedByClient != null)
        {
            return methodsSelectedByClient.Select(member => new AutoMember(member));
        }

        // Get the baseline members resolved by Bogus
        var autoMembers = GetMembers(type).Select(m => new AutoMember(m.Value)).ToList();

        foreach (var member in type.GetMembers(BindingFlags))
        {
            // Then check if any other members can be populated
            var autoMember = new AutoMember(member);

            if (autoMembers.Exists(baseMember => autoMember.Name == baseMember.Name))
            {
                continue;
            }

            // A readonly dictionary or collection member can use the Add() method
            if (autoMember.IsReadOnly && ReflectionHelper.IsDictionary(autoMember.Type))
            {
                autoMembers.Add(autoMember);
            }
            else if (autoMember.IsReadOnly && ReflectionHelper.IsCollection(autoMember.Type))
            {
                autoMembers.Add(autoMember);
            }
        }

        return autoMembers;
    }

    private object GenerateValue(
        Type                parentType,
        Type                generateType,
        string?             generateName,
        AutoGenerateContext context)
    {
        context.ParentType   = parentType;
        context.GenerateType = generateType;
        context.GenerateName = generateName;

        var generator = AutoGeneratorFactory.GetGenerator(context);
        var value     = generator.Generate(context);
        return value;
    }
}

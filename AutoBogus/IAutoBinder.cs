using System.Reflection;
using Bogus;

namespace AutoBogus;

/// <summary>
/// Provides direct functionality for generating and populating values.
/// It is low-level concept which roughly does what it is told and
/// typically <i>should not be used by the client code directly</i>.
/// Instead, clients should use <see cref="AutoFaker"/> (or <see cref="AutoFaker{TType}"/>),
/// which in its turn uses binder internally according to the provided
/// client configuration and/or Bogus rules.
///
/// <p>
/// Yet clients may provide their own implmentation of this interface to <see cref="AutoFaker{TType}"/>
/// in order to fine-grainly control faking mechanism.
/// </p>
/// </summary>
public interface IAutoBinder : IBinder
{
    /// <summary>
    /// Creates an <b>unpopulated</b> instance of the <typeparamref name="TType"/> type.
    /// The implementation should find an appropriate entry point
    /// (constructor / default value / predefined value set / etc) and
    /// create a new instance via it.
    ///
    /// <p>
    /// <b>Unpopulated</b> means that some of its members (public fields / properties / etc)
    /// may not have provided values which are to be generated and set later.
    /// The created unpopulated instance typically will be further additionaly populated
    /// via separate <see cref="PopulateInstance{TType}"/> call.
    /// </p>
    /// </summary>
    ///
    /// <typeparam name="TType">The type of the instance being generate.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the creation request.</param>
    /// <returns>The created unpopulated instance of the <typeparamref name="TType" /> type.</returns>
    TType CreateUnpopulatedInstance<TType>(AutoGenerateContext context);

    /// <summary>
    /// Populates the provided instance with generated values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to populate.</typeparam>
    /// <param name="instance">The instance to populate.</param>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the generate request.</param>
    /// <param name="members">
    /// An optional collection of members to populate.
    /// If <c>null</c>, all writable instance members will be populated.
    /// </param>
    /// <remarks>
    /// Due to the boxing nature of value types, the <paramref name="instance" /> parameter is an object.
    /// This means the populated values are applied to the provided instance and not a copy.
    /// </remarks>
    void PopulateInstance<TType>(object instance, AutoGenerateContext context, IEnumerable<MemberInfo>? members = null);
}

namespace AutoBogus.Internal;

/// <summary>
/// Contains all the isolated functionality for filtering members on
/// ones which should be popuplated with fake values and ones which not.
/// </summary>
internal static class PopulationTargetFiltering
{
    public static string GetSkipPathOfMember(Type type, string memberName)
    {
        return $"{type.FullName}.{memberName}";
    }

    public static bool ShoudPopulate(
        AutoGenerateContext context,
        Type                ownerType,
        string              memberName,
        Type                memberType)
    {
        return !context.Config.SkipTypes.Contains(memberType) &&
               !context.Config.SkipPaths.Contains(GetSkipPathOfMember(ownerType, memberName));
    }
}

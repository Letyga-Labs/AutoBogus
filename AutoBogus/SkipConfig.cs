namespace AutoBogus;

internal static class SkipConfig
{
    public static string MakePathForMember(Type type, string memberName)
    {
        return $"{type.FullName}.{memberName}";
    }

    public static bool ShouldSkip(
        AutoGenerateContext context,
        Type                ownerType,
        string              memberName,
        Type                memberType)
    {
        return context.Config.SkipTypes.Contains(memberType) ||
               context.Config.SkipPaths.Contains(MakePathForMember(ownerType, memberName));
    }
}

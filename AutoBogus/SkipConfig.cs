namespace AutoBogus;

internal static class SkipConfig
{
    public static string MakePathForMember(Type type, string memberName)
    {
        return $"{type.FullName}.{memberName}";
    }

    public static bool ShouldSkip(AutoGenerateContext context, Type type, string memberName)
    {
        return context.Config.SkipTypes.Contains(type) ||
               context.Config.SkipPaths.Contains(MakePathForMember(type, memberName));
    }
}

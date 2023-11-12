using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Tests.Models.Simple;

[SuppressMessage("Naming",           "CA1711:Identifiers should not have incorrect suffix")]
[SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes")]
public enum TestEnum
{
    Value1,
    Value2,
    Value3,
    Value4,
}

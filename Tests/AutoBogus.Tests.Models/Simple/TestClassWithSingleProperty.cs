using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Tests.Models.Simple;

public class TestClassWithSingleProperty<T>
{
    [SuppressMessage("Design",                               "CA1051:Do not declare visible instance fields")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private")]
    public T Value = default!;
}

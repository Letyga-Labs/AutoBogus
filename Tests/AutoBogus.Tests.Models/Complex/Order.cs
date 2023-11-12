using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Tests.Models.Complex;

public sealed class Order
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private")]
    [SuppressMessage("Design",                               "CA1051:Do not declare visible instance fields")]
    public DateTime Timestamp;

    public Order(int id, ICalculator? calculator)
    {
        Id         = id;
        Calculator = calculator;
    }

    public int          Id         { get; }
    public ICalculator? Calculator { get; }
    public Guid?        Code       { get; set; }
    public Status       Status     { get; set; }

    public IEnumerable<OrderItem>? Items       { get; set; }
    public DateTimeOffset          DateCreated { get; set; }

    [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
    public DiscountBase[]? Discounts { get; set; }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public ICollection<string>? Comments { get; set; }
}

using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Tests.Models.Complex;

public sealed class OrderItem
{
    public OrderItem(Product product)
    {
        Product = product;
    }

    public Product  Product  { get; }
    public Quantity Quantity { get; set; }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public IDictionary<int, decimal>? Discounts { get; set; }
}

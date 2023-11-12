using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Playground.Model;

[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
public sealed class Item
{
    public Guid                Id            { get; set; }
    public string              Name          { get; set; } = null!;
    public Product<int>        ProductInt    { get; set; } = null!;
    public Product<string>     ProductString { get; set; } = null!;
    public decimal             Price         { get; set; }
    public uint                Quantity      { get; set; }
    public Units               Units         { get; set; } = null!;
    public ItemStatus          Status        { get; set; }
    public Uri                 InfoLink      { get; set; } = null!;
    public ICollection<string> Comments      { get; set; } = null!;
    public User                ProcessedBy   { get; set; } = null!;
    public string              SupplierEmail { get; set; } = null!;
    public ITimestamp          Timestamp     { get; set; } = null!;
    public ISet<string>        Amendments    { get; set; } = null!;
}

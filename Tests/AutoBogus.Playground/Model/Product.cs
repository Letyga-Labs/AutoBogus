namespace AutoBogus.Playground.Model;

public class Product<TId>
{
    public TId          Id     { get; set; } = default!;
    public ProductCode  Code   { get; }      = new();
    public string       Name   { get; set; } = null!;
    public Product<TId> Parent { get; set; } = null!;

    protected ICollection<string> Notes { get; } = new List<string>();
}

namespace AutoBogus.Tests.Models.Complex;

public struct Quantity : IEquatable<Quantity>
{
    public int  Major { get; set; }
    public int? Minor { get; set; }

    public static bool operator ==(Quantity left, Quantity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Quantity left, Quantity right)
    {
        return !left.Equals(right);
    }

    public bool Equals(Quantity other)
    {
        return Major == other.Major && Minor == other.Minor;
    }

    public override bool Equals(object? obj)
    {
        return obj is Quantity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor);
    }
}

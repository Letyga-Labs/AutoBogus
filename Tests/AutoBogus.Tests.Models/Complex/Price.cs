namespace AutoBogus.Tests.Models.Complex;

public struct Price : IEquatable<Price>
{
    public Price(decimal amount, string units)
    {
        Amount = amount;
        Units  = units;
    }

    public decimal Amount { get; }
    public string  Units  { get; }

    public static bool operator ==(Price left, Price right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Price left, Price right)
    {
        return !left.Equals(right);
    }

    public bool Equals(Price other)
    {
        return Amount == other.Amount && Units == other.Units;
    }

    public override bool Equals(object? obj)
    {
        return obj is Price other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Units);
    }
}

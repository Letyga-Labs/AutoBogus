using System.Net;

namespace AutoBogus.Playground.Model;

public sealed class User
{
    public string    FirstName { get; set; } = null!;
    public string    LastName  { get; set; } = null!;
    public string    Email     { get; set; } = null!;
    public IPAddress Location  { get; set; } = null!;
}

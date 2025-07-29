namespace TodoApi.Models;

using Sieve.Attributes;

public class User
{
    public int Id { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string Email { get; set; } = string.Empty;
}

namespace Models;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Usr";
    public bool Seed { get; set; } = false;
    public DateTime CreateUtc { get; set; } = DateTime.UtcNow;
}
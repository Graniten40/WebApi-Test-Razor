namespace DbModels;

public class UserDbM
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = "usr";
    public bool Seeded { get; set; } = false;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

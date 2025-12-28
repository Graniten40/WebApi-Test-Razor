namespace Models.DTO;

public class UserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Usr";
    public DateTime CreateUtc { get; set; }
}

public class CreateUserDto
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Usr";
}

public class UpdateUserDto
{
    public string Email { get; set; } 
    public string? Password { get; set; }
    public string Role { get; set; } 
}
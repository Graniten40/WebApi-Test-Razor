namespace AppRazor.Models;

public class FriendInLocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

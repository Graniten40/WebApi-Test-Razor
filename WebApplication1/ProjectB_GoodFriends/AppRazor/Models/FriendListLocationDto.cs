namespace AppRazor.Models;

public class FriendListLocationDto
{
    public Guid FriendId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

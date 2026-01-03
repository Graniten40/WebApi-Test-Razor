namespace AppRazor.Models;

public class FriendUpdateRequestDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? Birthday { get; set; }
    public AddressUpdateRequestDto? Address { get; set; }
}

namespace AppRazor.Models;

public class FriendsByCountryDto
{
    public string Country { get; set; } = "";
    public int TotalFriends { get; set; }
    public int Cities { get; set; }
}

public class FriendsByCountryCityDto
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public int NrFriends { get; set; }
}

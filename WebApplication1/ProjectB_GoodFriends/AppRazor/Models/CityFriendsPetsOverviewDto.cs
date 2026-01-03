namespace AppRazor.Models;

public class CityFriendsPetsOverviewDto
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public int FriendsCount { get; set; }
    public int PetsCount { get; set; }
}

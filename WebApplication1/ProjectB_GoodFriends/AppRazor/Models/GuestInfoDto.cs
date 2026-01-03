namespace AppRazor.Models;

public class GuestInfoDto
{
    public string ConnectionString { get; set; } = "";
    public GuestInfoItem Item { get; set; } = new();
}

public class GuestInfoItem
{
    public DbInfo Db { get; set; } = new();
    public List<FriendGroupInfo> Friends { get; set; } = new();
}

public class DbInfo
{
    public int NrSeededFriends { get; set; }
    public int NrFriendsWithAddress { get; set; }
    public int NrSeededPets { get; set; }
    public int NrSeededQuotes { get; set; }
}

public class FriendGroupInfo
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public int NrFriends { get; set; }
}

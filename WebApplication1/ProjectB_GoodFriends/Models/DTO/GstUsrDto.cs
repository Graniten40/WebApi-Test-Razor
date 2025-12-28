namespace Models.DTO;

public class GstUsrInfoDbDto
{
    public int NrSeededFriends { get; set; } = 0;
    public int NrUnseededFriends { get; set; } = 0;
    public int NrFriendsWithAddress { get; set; } = 0;

    public int NrSeededAddresses { get; set; } = 0;
    public int NrUnseededAddresses { get; set; } = 0;

    public int NrSeededPets { get; set; } = 0;
    public int NrUnseededPets { get; set; } = 0;

    public int NrSeededQuotes { get; set; } = 0;
    public int NrUnseededQuotes { get; set; } = 0;
}

public class GstUsrInfoFriendsDto
{
    public string Country { get; set; } = null;
    public string City { get; set; } = null;
    public int NrFriends { get; set; } = 0;
}

public class GstUsrInfoPetsDto
{
    public string Country { get; set; } = null;
    public string City { get; set; } = null;
    public int NrPets { get; set; } = 0;
}

public class GstUsrInfoQuotesDto
{
    public string Author { get; set; } = null;
    public int NrQuotes { get; set; } = 0;
}

public class GstUsrInfoAllDto
{
    public GstUsrInfoDbDto Db { get; set; } = null;
    public List<GstUsrInfoFriendsDto> Friends { get; set; } = null;
    public List<GstUsrInfoPetsDto> Pets { get; set; } = null;
    public List<GstUsrInfoQuotesDto> Quotes { get; set; } = null;
}



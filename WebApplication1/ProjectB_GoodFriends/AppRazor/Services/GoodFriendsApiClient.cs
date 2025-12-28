using System.Net.Http.Json;

public class GoodFriendsApiClient
{
    private readonly HttpClient _http;

    public GoodFriendsApiClient(HttpClient http)
    {
        _http = http;
    }

    // Ping / test
    public async Task<string> GetGuestInfoAsync()
    {
        return await _http.GetStringAsync("api/Guest/Info");
    }

    // Seed database
    public async Task SeedAsync()
    {
        var response = await _http.GetAsync("api/Admin/Seed");
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveSeedAsync()
    {
        var response = await _http.GetAsync("api/Admin/RemoveSeed");
        response.EnsureSuccessStatusCode();
    }
}

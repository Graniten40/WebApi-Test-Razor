using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Overview;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GoodFriendsApiClient _api;

    // Existing overview data
    public List<FriendsByCountryDto> Summary { get; private set; } = [];
    public List<FriendsByCountryCityDto> Details { get; private set; } = [];

    // NEW: selected country + per-city overview (friends + pets)
    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    public List<CityFriendsPetsOverviewDto> Cities { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, GoodFriendsApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    public async Task OnGetAsync()
    {
        try
        {
            // Always load the existing overview lists
            Summary = await _api.GetFriendsByCountryAsync();
            Details = await _api.GetFriendsByCountryCityAsync();

            // If user picked a country -> load city overview (friends + pets)
            if (!string.IsNullOrWhiteSpace(Country))
            {
                Cities = await _api.GetCityFriendsPetsOverviewAsync(Country);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load overview data");
            ErrorMessage = "Could not load Overview data from API.";
        }
    }
}

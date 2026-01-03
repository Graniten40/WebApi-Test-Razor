using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.FriendsByLocation;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GoodFriendsApiClient _api;

    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    public List<FriendListLocationDto> Friends { get; private set; } = [];
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
            // Ladda bara när user har fyllt i nåt filter
            if (!string.IsNullOrWhiteSpace(Country) || !string.IsNullOrWhiteSpace(City))
                Friends = await _api.ListFriendsAsync(Country, City);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load friends by location");
            ErrorMessage = "Could not load friends list from API.";
        }
    }
}

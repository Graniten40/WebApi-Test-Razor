using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.FriendsByLocation;

/// <summary>
/// PageModel för FriendsByLocation/Index.
/// Ansvarar för att lista friends baserat på Country och/eller City.
/// </summary>
public class IndexModel : PageModel
{
    // Logger används för felsökning och spårbarhet
    private readonly ILogger<IndexModel> _logger;

    // API-klient som anropar backend för listning baserat på plats
    private readonly GoodFriendsApiClient _api;

    /// <summary>
    /// Land som filter. Bindas från querystring via GET.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    /// <summary>
    /// Stad som filter. Bindas från querystring via GET.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    /// <summary>
    /// Resultatlista som renderas i vyn.
    /// Initieras tom för att undvika null-checkar i cshtml.
    /// </summary>
    public List<FriendListLocationDto> Friends { get; private set; } = [];

    /// <summary>
    /// Felmeddelande som visas i vyn om API-anropet misslyckas.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, GoodFriendsApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    /// <summary>
    /// GET: laddar friends baserat på angivna filter.
    /// API-anrop görs endast om användaren angett Country och/eller City,
    /// för att undvika att lista "allt" vid första sidladdning.
    /// </summary>
    public async Task OnGetAsync()
    {
        try
        {
            // Ladda endast när minst ett filter är ifyllt
            if (!string.IsNullOrWhiteSpace(Country) || !string.IsNullOrWhiteSpace(City))
            {
                Friends = await _api.ListFriendsAsync(Country, City);
            }
        }
        catch (Exception ex)
        {
            // Logga tekniskt fel för felsökning
            _logger.LogError(ex, "Failed to load friends by location");

            // Visa ett användarvänligt felmeddelande i UI
            ErrorMessage = "Could not load friends list from API.";
        }
    }
}

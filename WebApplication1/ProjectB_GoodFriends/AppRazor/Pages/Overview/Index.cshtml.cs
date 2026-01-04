using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Overview;

/// <summary>
/// PageModel för Overview-sidan.
/// Visar:
/// - Sammanfattning av friends per land
/// - Detaljer per land + stad
/// - (Valfritt) drill-down per stad med antal friends och pets för valt land
/// </summary>
public class IndexModel : PageModel
{
    // Logger för felsökning och spårbarhet
    private readonly ILogger<IndexModel> _logger;

    // API-klient som hämtar översiktsdata från backend
    private readonly GoodFriendsApiClient _api;

    /// <summary>
    /// Sammanfattning per land (antal friends och antal städer).
    /// Används i första tabellen i vyn.
    /// </summary>
    public List<FriendsByCountryDto> Summary { get; private set; } = [];

    /// <summary>
    /// Full detaljlista per land + stad.
    /// Används i "Details – Country + City"-tabellen.
    /// </summary>
    public List<FriendsByCountryCityDto> Details { get; private set; } = [];

    /// <summary>
    /// Vald country från querystring.
    /// När denna är satt visas city-översikten (friends + pets) i vyn.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    /// <summary>
    /// Per-stad-översikt för valt land:
    /// antal friends och antal pets per stad.
    /// </summary>
    public List<CityFriendsPetsOverviewDto> Cities { get; private set; } = [];

    /// <summary>
    /// Felmeddelande som visas i vyn om något går fel vid laddning.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, GoodFriendsApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    /// <summary>
    /// GET: laddar översiktsdata.
    /// - Summary och Details laddas alltid
    /// - Cities laddas endast om användaren valt ett land (querystring: country)
    /// </summary>
    public async Task OnGetAsync()
    {
        try
        {
            // Ladda alltid grundläggande översikter
            Summary = await _api.GetFriendsByCountryAsync();
            Details = await _api.GetFriendsByCountryCityAsync();

            // Om användaren valt ett land: ladda stadsspecifik översikt
            if (!string.IsNullOrWhiteSpace(Country))
            {
                Cities = await _api.GetCityFriendsPetsOverviewAsync(Country);
            }
        }
        catch (Exception ex)
        {
            // Logga tekniskt fel för felsökning
            _logger.LogError(ex, "Failed to load overview data");

            // Visa användarvänligt felmeddelande i UI
            ErrorMessage = "Could not load Overview data from API.";
        }
    }
}

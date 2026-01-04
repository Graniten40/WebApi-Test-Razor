using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

/// <summary>
/// PageModel för Friends/Index.
/// Ansvarar för:
/// - fri textsökning (namn/email)
/// - paging
/// - anrop till API för att hämta listan av friends
/// </summary>
public class IndexModel : PageModel
{
    // Logger används för felsökning och spårbarhet
    private readonly ILogger<IndexModel> _logger;

    // API-klient som pratar med backend
    private readonly GoodFriendsApiClient _api;

    /// <summary>
    /// Fri textsökning som skickas via querystring (GET).
    /// Används för att filtrera på namn och email (inte land, trots namnet).
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    /// <summary>
    /// Aktuell sida (0-baserad).
    /// Viktigt: vyn använder PageNr konsekvent vid paging.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int PageNr { get; set; } = 0;

    /// <summary>
    /// Resultatet från API:t inklusive paging-information.
    /// </summary>
    public ResponsePageDto<FriendListItemDto>? Result { get; private set; }

    /// <summary>
    /// Felmeddelande som visas i vyn om något går fel.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, GoodFriendsApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    /// <summary>
    /// GET: hämtar friends baserat på sökfilter och sida.
    /// Alla parametrar kommer från querystring via SupportsGet.
    /// </summary>
    public async Task OnGetAsync()
    {
        try
        {
            // Backend tillåter endast bokstäver, siffror och mellanslag i filter.
            // Vi sanerar input här för att undvika ogiltiga tecken och onödiga API-fel.
            static string Sanitize(string input) =>
                new string(input
                    .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                    .ToArray());

            string? filter = null;

            // Om användaren har angett något i sökfältet
            if (!string.IsNullOrWhiteSpace(Country))
                filter = Sanitize(Country.Trim());

            // Anropa API:t med paging och ev. filter
            Result = await _api.ReadFriendsAsync(
                seeded: true,
                flat: true,
                filter: filter,
                pageNr: PageNr,
                pageSize: 50);
        }
        catch (Exception ex)
        {
            // Logga felet för felsökning
            _logger.LogError(ex, "Failed to read friends");

            // Visa felmeddelande i UI (ok under utveckling/inlämning)
            ErrorMessage = ex.Message;
        }
    }
}

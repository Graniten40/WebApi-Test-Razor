using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GoodFriendsApiClient _api;

    // Används som fri textsökning (namn/email)
    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    // Viktigt: vyn ska använda PageNr (inte "Page")
    [BindProperty(SupportsGet = true)]
    public int PageNr { get; set; } = 0;

    public ResponsePageDto<FriendListItemDto>? Result { get; private set; }
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
            // Backend tillåter endast bokstäver, siffror och mellanslag
            static string Sanitize(string input) =>
                new string(input
                    .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                    .ToArray());

            string? filter = null;

            if (!string.IsNullOrWhiteSpace(Country))
                filter = Sanitize(Country.Trim());

            Result = await _api.ReadFriendsAsync(
                seeded: true,
                flat: true,
                filter: filter,
                pageNr: PageNr,
                pageSize: 50);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read friends");
            ErrorMessage = ex.Message; // OK för felsökning / kan bytas till generell text senare
        }
    }
}

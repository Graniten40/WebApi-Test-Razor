using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

/// <summary>
/// PageModel för en friend-details-sida.
/// Visar friend och hanterar radering via en återanvändbar "DeleteItem"-handler.
/// </summary>
public class DetailsModel : PageModel
{
    // API-klient som anropar WebAPI:t (CRUD för friends/pets/quotes).
    private readonly GoodFriendsApiClient _api;

    public DetailsModel(GoodFriendsApiClient api) => _api = api;

    /// <summary>
    /// FriendId kan bindas från querystring/route även vid GET,
    /// tack vare SupportsGet = true.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    /// <summary>
    /// Objektet som sidan renderar. Sätts i LoadAsync().
    /// </summary>
    public FriendDetailsDto? Friend { get; private set; }

    /// <summary>
    /// TempData används för "flash message" efter redirect (PRG-mönster).
    /// </summary>
    [TempData]
    public string? FlashMessage { get; set; }

    /// <summary>
    /// Felmeddelande som kan visas i vyn vid API-fel.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // -----------------------------
    // GET
    // -----------------------------

    /// <summary>
    /// Laddar details-sidan för vald friend.
    /// friendId kommer från route/query beroende på hur sidan anropas.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid friendId)
    {
        // Spara friendId i property så att vyn kan använda det (t.ex. i länkar/formulär).
        FriendId = friendId;

        // Gemensam laddningsmetod så vi kan återanvända logik (även efter POST-fel).
        return await LoadAsync(friendId);
    }

    // -----------------------------
    // DELETE (modal) - reuse one modal
    // POST: ?handler=DeleteItem  (friendId, kind, itemId)
    // -----------------------------

    /// <summary>
    /// Gemensam delete-handler som kan radera olika typer (pet/quote) baserat på parametern "kind".
    /// Fördelen: man kan återanvända en modal + ett formulär i vyn istället för två separata handlers.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteItemAsync(Guid friendId, string kind, Guid itemId)
    {
        try
        {
            // Guard: om data saknas eller är fel så gör vi inget – men redirectar tillbaka till sidan.
            if (friendId == Guid.Empty || itemId == Guid.Empty || string.IsNullOrWhiteSpace(kind))
                return RedirectToPage(new { friendId });

            // Normalisera input så jämförelser blir stabila (" Pet " → "pet").
            kind = kind.Trim().ToLowerInvariant();

            if (kind == "pet")
            {
                // DeletePetAsync tar EN parameter: petId
                await _api.DeletePetAsync(itemId);
                FlashMessage = "Pet deleted.";
            }
            else if (kind == "quote")
            {
                // DeleteQuoteAsync tar EN parameter: quoteId
                await _api.DeleteQuoteAsync(itemId);
                FlashMessage = "Quote deleted.";
            }
            else
            {
                // Okänt "kind" → vi gör ingen radering men ger feedback.
                FlashMessage = "Nothing deleted (unknown type).";
            }

            // PRG (Post/Redirect/Get): undvik dubbel-delete om användaren refreshar sidan.
            return RedirectToPage(new { friendId });
        }
        catch (HttpRequestException ex)
        {
            // API-relaterade fel: visa feltext och rendera sidan igen med laddad data.
            ErrorMessage = ex.Message;
            FriendId = friendId;
            await LoadAsync(friendId);
            return Page();
        }
        catch (Exception ex)
        {
            // Generell fallback: gör sidan robust även vid oväntade fel.
            ErrorMessage = ex.Message;
            FriendId = friendId;
            await LoadAsync(friendId);
            return Page();
        }
    }

    // -----------------------------
    // Shared loader
    // -----------------------------

    /// <summary>
    /// Hämtar friend details och ev. kompletterande data.
    /// Separata try/catch gör att sidan kan renderas även om en del-anrop misslyckas.
    /// </summary>
    private async Task<IActionResult> LoadAsync(Guid friendId)
    {
        try
        {
            Friend = await _api.GetFriendDetailsAsync(friendId);
            if (Friend == null)
            {
                ErrorMessage = "Friend not found.";
                return Page();
            }

            return Page();
        }
        catch (HttpRequestException ex)
        {
            // Vid API-fel renderar vi sidan och visar ErrorMessage i UI.
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}

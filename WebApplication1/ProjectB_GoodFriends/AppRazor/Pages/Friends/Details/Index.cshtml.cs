using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends.Details;

/// <summary>
/// Razor PageModel för Friends/Details.
/// Ansvarar för:
/// - Visa friend details (GET)
/// - Skapa Pet och Quote (POST handlers)
/// - Radera Pet och Quote (POST handlers + modal confirmation)
/// - Ladda om sidan med uppdaterad data efter ändringar
/// </summary>
public class IndexModel : PageModel
{
    // API-klient som pratar med WebAPI:t (se Services/GoodFriendsApiClient)
    private readonly GoodFriendsApiClient _api;
    // Logger används för felsökning/spårbarhet (bra i inlämning för att visa diagnos)
    private readonly ILogger<IndexModel> _logger;

    // Viktigt: detta måste vara konsekvent med hur API:t hämtar data.
    // Om GetFriendDetailsAsync hämtar seeded:true så ska även relationsanropen använda samma läge,
    // annars kan du få "Friend finns men pets/quotes saknas" eller tvärtom..
    private const bool SeededMode = true; // <-- ändra till false om du vill jobba på "riktig" data

    /// <summary>
    /// Data som renderas i vyn. Sätts i LoadAsync().
    /// </summary>
    public FriendDetailsDto? Friend { get; private set; }
    /// <summary>
    /// Feltext som visas i vyn (t.ex. API-fel eller valideringsfel).
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// "Flash message" mellan requests via TempData (ex: efter Add/Delete + Redirect).
    /// </summary>
    [TempData]
    public string? FlashMessage { get; set; }

    /// <summary>
    /// Bindas från formuläret "Add pet".
    /// </summary>
    [BindProperty]
    public PetCreateDto NewPet { get; set; } = new();

    /// <summary>
    /// Bindas från formuläret "Add pet".
    /// </summary>
    [BindProperty]
    public QuoteCreateDto NewQuote { get; set; } = new();

    public IndexModel(GoodFriendsApiClient api, ILogger<IndexModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    // -------------------------------------------------
    // GET
    // -------------------------------------------------
    /// <summary>
    /// Hämtar och visar friend details.
    /// friendId kommer från route: @page "{friendId:guid}".
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid friendId)
    {
        _logger.LogInformation("GET Details for friendId={FriendId}", friendId);
        return await LoadAsync(friendId);
    }

    // -------------------------------------------------
    // ADD PET
    // -------------------------------------------------
    /// <summary>
    /// Handler för POST från "Add pet"-formuläret.
    /// Viktigt: eftersom sidan har flera formulär måste vi se till att vi validerar rätt modell.
    /// </summary>
    public async Task<IActionResult> OnPostAddPetAsync(Guid friendId)
    {
        _logger.LogInformation(
            "AddPet called. friendId={FriendId}, Name={Name}, Kind={Kind}, Mood={Mood}",
            friendId, NewPet.Name, NewPet.Kind, NewPet.Mood);

        // Guard: route saknas eller är fel
        if (friendId == Guid.Empty)
            return NotFound();

        // När sidan har flera formulär: ModelState kan innehålla fel från "fel" form.
        // Därför tar vi bort Quote-nycklar så att endast Pet valideras.
        // Både prefixade och oprefixade nycklar rensas för robusthet.
        RemoveKeys(
            "NewQuote.Text", "NewQuote.Author",
            "Text", "Author"
        );

        // Validera ENDAST NewPet (via DataAnnotations på PetCreateDto)
        if (!TryValidateModel(NewPet, nameof(NewPet)))
        {
            ErrorMessage = BuildModelStateError("Pet validation failed");
            _logger.LogWarning("AddPet invalid: {Errors}", ErrorMessage);
            // Ladda om sidan med befintlig data så användaren ser felmeddelanden direkt
            return await LoadAsync(friendId);
        }

        try
        {
            // Skapa pet via API
            await _api.AddPetAsync(friendId, NewPet);

            // TempData överlever Redirect → användaren ser "Pet added."
            FlashMessage = "Pet added.";

            // PRG-mönster (Post/Redirect/Get) för att undvika dubbelpost vid refresh
            return RedirectToPage(new { friendId });
        }

        // API-fel ska inte krascha sidan – vi visar feltext och laddar om
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AddPet API call failed");
            ErrorMessage = ex.Message;
            return await LoadAsync(friendId);
        }
    }

    // -------------------------------------------------
    // ADD QUOTE
    // -------------------------------------------------
    /// <summary>
    /// Handler för POST från "Add quote"-formuläret.
    /// Samma princip som AddPet: rensa ModelState för andra formuläret.
    /// </summary>
    public async Task<IActionResult> OnPostAddQuoteAsync(Guid friendId)
    {
        _logger.LogInformation(
            "AddQuote called. friendId={FriendId}, Text={Text}, Author={Author}",
            friendId, NewQuote.Text, NewQuote.Author);

        if (friendId == Guid.Empty)
            return NotFound();

        // Rensa pet-validering (både prefixade och oprefixade nycklar)
        RemoveKeys(
            "NewPet.Name", "NewPet.Kind", "NewPet.Mood",
            "Name", "Kind", "Mood"
        );

        // Validera endast NewQuote
        if (!TryValidateModel(NewQuote, nameof(NewQuote)))
        {
            ErrorMessage = BuildModelStateError("Quote validation failed");
            _logger.LogWarning("AddQuote invalid: {Errors}", ErrorMessage);
            return await LoadAsync(friendId);
        }

        try
        {
            await _api.AddQuoteAsync(friendId, NewQuote);
            FlashMessage = "Quote added.";
            return RedirectToPage(new { friendId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AddQuote API call failed");
            ErrorMessage = ex.Message;
            return await LoadAsync(friendId);
        }
    }

    // -------------------------------------------------
    // DELETE PET
    // -------------------------------------------------
    /// <summary>
    /// Raderar en pet. Triggas av modalens POST-form (hidden input petId).
    /// </summary>
    public async Task<IActionResult> OnPostDeletePetAsync(Guid friendId, Guid petId)
    {
        _logger.LogInformation("DeletePet called. friendId={FriendId}, petId={PetId}", friendId, petId);

        if (friendId == Guid.Empty)
            return NotFound();

        // Om hidden input inte blev satt (ex. JS misslyckades) hanterar vi det säkert
        if (petId == Guid.Empty)
        {
            FlashMessage = "Nothing to delete.";
            return RedirectToPage(new { friendId });
        }

        try
        {
            await _api.DeletePetAsync(petId);
            FlashMessage = "Pet deleted.";
            return RedirectToPage(new { friendId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "DeletePet API call failed");
            ErrorMessage = ex.Message;
            return await LoadAsync(friendId);
        }
    }

    // -------------------------------------------------
    // DELETE QUOTE
    // -------------------------------------------------
    /// <summary>
    /// Raderar en quote. Triggas av modalens POST-form (hidden input quoteId).
    /// </summary>
    public async Task<IActionResult> OnPostDeleteQuoteAsync(Guid friendId, Guid quoteId)
    {
        _logger.LogInformation("DeleteQuote called. friendId={FriendId}, quoteId={QuoteId}", friendId, quoteId);

        if (friendId == Guid.Empty)
            return NotFound();

        if (quoteId == Guid.Empty)
        {
            FlashMessage = "Nothing to delete.";
            return RedirectToPage(new { friendId });
        }

        try
        {
            await _api.DeleteQuoteAsync(quoteId);
            FlashMessage = "Quote deleted.";
            return RedirectToPage(new { friendId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "DeleteQuote API call failed");
            ErrorMessage = ex.Message;
            return await LoadAsync(friendId);
        }
    }

    // -------------------------------------------------
    // Shared loader
    // -------------------------------------------------
    /// <summary>
    /// Gemensam laddningsmetod för att hämta friend + relationer.
    /// Anropas från både GET och vid POST-fel så sidan alltid kan renderas.
    /// </summary>
    private async Task<IActionResult> LoadAsync(Guid friendId)
    {
        _logger.LogInformation("Loading friend details for friendId={FriendId}", friendId);

        if (friendId == Guid.Empty)
            return NotFound();

        try
        {
            // Hämtar huvudobjektet (friend). Relationer laddas separat nedan.
            Friend = await _api.GetFriendDetailsAsync(friendId);

            if (Friend == null)
            {
                // Vi returnerar Page() istället för NotFound() för att kunna visa ErrorMessage i UI:t
                ErrorMessage = $"Friend not found for id={friendId} (seeded mismatch?)";
                return Page(); // ✅ inte NotFound()
            }

            // Relationsdata laddas i separata try/catch så att sidan kan renderas även om en del fallerar.
            // Detta ger bättre UX och mer robust demo vid inlämning.

            try
            {
                var quotes = await _api.GetQuotesForFriendAsync(friendId, seeded: SeededMode, flat: false);
                Friend.Quotes = quotes.ToList();
                _logger.LogInformation("Loaded {Count} quotes", Friend.Quotes.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load quotes");
                AppendError($"Could not load quotes: {ex.Message}");
                Friend.Quotes = new();
            }

            try
            {
                var pets = await _api.GetPetsForFriendAsync(friendId, seeded: SeededMode, flat: false);
                Friend.Pets = pets.ToList();
                _logger.LogInformation("Loaded {Count} pets", Friend.Pets.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to load pets");
                AppendError($"Could not load pets: {ex.Message}");
                Friend.Pets = new();
            }

            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to load friend details");
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    // -------------------------------------------------
    // Helpers
    // -------------------------------------------------
    /// <summary>
    /// Tar bort specifika ModelState-nycklar.
    /// Används här eftersom sidan har flera formulär och vi vill validera en modell i taget.
    /// </summary>
    private void RemoveKeys(params string[] keys)
    {
        foreach (var k in keys)
            ModelState.Remove(k);
    }

    /// <summary>
    /// Bygger en läsbar felsträng av ModelState (bra för loggning och visning under utveckling).
    /// </summary>
    private string BuildModelStateError(string prefix)
    {
        var errors = ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => $"{kvp.Key}: {string.Join(" | ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}");

        return $"{prefix}: {string.Join(" || ", errors)}";
    }

    private void AppendError(string msg)
    {
        ErrorMessage = string.IsNullOrWhiteSpace(ErrorMessage)
            ? msg
            : $"{ErrorMessage} | {msg}";
    }
}

using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends.Details;

public class IndexModel : PageModel
{
    private readonly GoodFriendsApiClient _api;
    private readonly ILogger<IndexModel> _logger;

    public FriendDetailsDto? Friend { get; private set; }
    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? FlashMessage { get; set; }

    [BindProperty]
    public PetCreateDto NewPet { get; set; } = new();

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
    public async Task<IActionResult> OnGetAsync(Guid friendId)
    {
        _logger.LogInformation("GET Details for friendId={FriendId}", friendId);
        return await LoadAsync(friendId);
    }

    // -------------------------------------------------
    // ADD PET
    // -------------------------------------------------
    public async Task<IActionResult> OnPostAddPetAsync(Guid friendId)
    {
        _logger.LogInformation(
            "AddPet called. friendId={FriendId}, Name={Name}, Kind={Kind}, Mood={Mood}",
            friendId, NewPet.Name, NewPet.Kind, NewPet.Mood);

        if (friendId == Guid.Empty)
        {
            _logger.LogWarning("AddPet aborted: friendId is empty");
            return NotFound();
        }

        // ✅ Validera bara NewPet utan att rensa hela ModelState
        ModelState.Remove("NewQuote.Text");
        ModelState.Remove("NewQuote.Author");

        if (!TryValidateModel(NewPet, nameof(NewPet)))
        {
            ErrorMessage = BuildModelStateError("Pet validation failed");
            _logger.LogWarning("AddPet invalid: {Errors}", ErrorMessage);
            return await LoadAsync(friendId);
        }

        try
        {
            await _api.AddPetAsync(friendId, NewPet);

            FlashMessage = "Pet added.";
            return RedirectToPage(new { friendId });
        }
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
    public async Task<IActionResult> OnPostAddQuoteAsync(Guid friendId)
    {
        _logger.LogInformation(
            "AddQuote called. friendId={FriendId}, Text={Text}, Author={Author}",
            friendId, NewQuote.Text, NewQuote.Author);

        if (friendId == Guid.Empty)
        {
            _logger.LogWarning("AddQuote aborted: friendId is empty");
            return NotFound();
        }

        // ✅ Validera bara NewQuote utan att rensa hela ModelState
        ModelState.Remove("NewPet.Name");
        ModelState.Remove("NewPet.Kind");
        ModelState.Remove("NewPet.Mood");

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
    public async Task<IActionResult> OnPostDeletePetAsync(Guid friendId, Guid petId)
    {
        _logger.LogInformation("DeletePet called. friendId={FriendId}, petId={PetId}", friendId, petId);

        if (friendId == Guid.Empty)
            return NotFound();

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
    private async Task<IActionResult> LoadAsync(Guid friendId)
    {
        _logger.LogInformation("Loading friend details for friendId={FriendId}", friendId);

        if (friendId == Guid.Empty)
            return NotFound();

        try
        {
            Friend = await _api.GetFriendDetailsAsync(friendId);

            if (Friend == null)
            {
                ErrorMessage = $"Friend not found for id={friendId} (seeded mismatch?)";
                return Page(); // ✅ inte NotFound() → annars får du Chrome-404
            }

            // ✅ Ladda quotes/pets, men låt sidan renderas även om de misslyckas
            try
            {
                var quotes = await _api.GetQuotesForFriendAsync(friendId, seeded: false, flat: false);
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
                var pets = await _api.GetPetsForFriendAsync(friendId, seeded: false, flat: false);
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

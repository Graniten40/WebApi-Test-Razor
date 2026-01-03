using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

public class DetailsModel : PageModel
{
    private readonly GoodFriendsApiClient _api;

    public DetailsModel(GoodFriendsApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    public FriendDetailsDto? Friend { get; private set; }

    [TempData]
    public string? FlashMessage { get; set; }

    public string? ErrorMessage { get; private set; }

    // -----------------------------
    // GET
    // -----------------------------
    public async Task<IActionResult> OnGetAsync(Guid friendId)
    {
        FriendId = friendId;
        return await LoadAsync(friendId);
    }

    // -----------------------------
    // DELETE (modal) - reuse one modal
    // POST: ?handler=DeleteItem  (friendId, kind, itemId)
    // -----------------------------
    public async Task<IActionResult> OnPostDeleteItemAsync(Guid friendId, string kind, Guid itemId)
    {
        try
        {
            if (friendId == Guid.Empty || itemId == Guid.Empty || string.IsNullOrWhiteSpace(kind))
                return RedirectToPage(new { friendId });

            kind = kind.Trim().ToLowerInvariant();

            if (kind == "pet")
            {
                // ✅ EN parameter (petId)
                await _api.DeletePetAsync(itemId);
                FlashMessage = "Pet deleted.";
            }
            else if (kind == "quote")
            {
                // ✅ EN parameter (quoteId)
                await _api.DeleteQuoteAsync(itemId);
                FlashMessage = "Quote deleted.";
            }
            else
            {
                FlashMessage = "Nothing deleted (unknown type).";
            }

            return RedirectToPage(new { friendId });
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            FriendId = friendId;
            await LoadAsync(friendId);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            FriendId = friendId;
            await LoadAsync(friendId);
            return Page();
        }
    }

    // -----------------------------
    // Shared loader
    // -----------------------------
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

            // ✅ Om quotes INTE ingår i details-endpointen:
            try
            {
                var quotes = await _api.GetQuotesForFriendAsync(friendId);
                Friend.Quotes = quotes.ToList();
            }
            catch
            {
                // Om endpointen inte finns / inte behövs, ignorera utan att krascha sidan
            }

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}

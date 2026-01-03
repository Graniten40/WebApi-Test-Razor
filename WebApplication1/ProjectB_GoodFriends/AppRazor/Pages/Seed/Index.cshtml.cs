using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Seed;

public class IndexModel : PageModel
{
    private readonly GoodFriendsApiClient _api;

    public string? Message { get; set; }

    public IndexModel(GoodFriendsApiClient api)
    {
        _api = api;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostSeedAsync()
    {
        await _api.SeedAsync();
        return RedirectToPage("/Index"); // tillbaka till Home som visar counts
    }

    public async Task<IActionResult> OnPostRemoveSeedAsync()
    {
        await _api.RemoveSeedAsync();
        return RedirectToPage("/Index");
    }
}

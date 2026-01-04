using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Seed;

/// <summary>
/// PageModel för Seed-sidan.
/// Ansvarar för att:
/// - skapa seed/test-data i databasen
/// - ta bort seed-data
/// </summary>
public class IndexModel : PageModel
{
    // API-klient som utför seed-operationer mot backend
    private readonly GoodFriendsApiClient _api;

    /// <summary>
    /// Meddelande som kan användas för feedback i UI (om man vill visa info direkt).
    /// </summary>
    public string? Message { get; set; }

    public IndexModel(GoodFriendsApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// GET: renderar Seed-sidan.
    /// Innehåller ingen logik eftersom sidan endast visar knappar.
    /// </summary>
    public void OnGet() { }

    /// <summary>
    /// POST-handler för att skapa seed-data i databasen.
    /// Efter utfört seed redirectas användaren till startsidan (PRG-mönster).
    /// </summary>
    public async Task<IActionResult> OnPostSeedAsync()
    {
        await _api.SeedAsync();

        // Redirect till Home/Index som visar sammanfattande data (counts)
        return RedirectToPage("/Index");
    }

    /// <summary>
    /// POST-handler för att ta bort seed-data från databasen.
    /// Även här används redirect för att undvika dubbel-post vid refresh.
    /// </summary>
    public async Task<IActionResult> OnPostRemoveSeedAsync()
    {
        await _api.RemoveSeedAsync();
        return RedirectToPage("/Index");
    }
}

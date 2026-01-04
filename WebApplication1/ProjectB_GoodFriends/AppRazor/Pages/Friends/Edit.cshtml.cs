using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

/// <summary>
/// PageModel för att redigera en Friend.
/// - GET: hämtar FriendDetails och mappar till FriendEditDto (edit-vänligt format)
/// - POST: antingen visar adressfält (om adress saknas) eller skickar uppdatering till API
/// - Hanterar både client-side (ModelState) och server-side validering (från API)
/// </summary>
public class EditModel : PageModel
{
    // API-klient som anropar WebAPI:t (read + update)
    private readonly GoodFriendsApiClient _api;

    public EditModel(GoodFriendsApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Modellen som formuläret binder till i cshtml (asp-for="Friend.*").
    /// </summary>
    [BindProperty]
    public FriendEditDto Friend { get; set; } = new();

    /// <summary>
    /// Generellt felmeddelande (t.ex. om API:t returnerar ett icke-valideringsfel).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// GET: hämtar details och mappar till edit-modellen.
    /// Vi mappar manuellt för att inte råka skicka med oönskade fält (separation mellan read/edit DTO).
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var details = await _api.GetFriendDetailsAsync(id);
        if (details is null)
            return NotFound();

        // Mapping: DetailsDto -> EditDto (endast fält som ska redigeras)
        Friend = new FriendEditDto
        {
            FriendId = details.FriendId,
            FirstName = details.FirstName,
            LastName = details.LastName,
            Email = details.Email,
            Birthday = details.Birthday,

            // Adress är optional → om details saknar adress ska edit-modellen också sakna den
            Address = details.Address is null ? null : new AddressEditDto
            {
                AddressId = details.Address.AddressId,
                StreetAddress = details.Address.StreetAddress,
                ZipCode = details.Address.ZipCode,
                City = details.Address.City,
                Country = details.Address.Country
            }
        };

        // Om du vill att address-fälten alltid ska visas:
        // Friend.Address ??= new AddressEditDto();

        return Page();
    }

    /// <summary>
    /// POST: hanterar två scenarion:
    /// 1) addAddress == "true" → användaren vill visa adressfält (utan att spara än)
    /// 2) annars → validera och skicka update till API
    /// </summary>
    public async Task<IActionResult> OnPostAsync(string? addAddress)
    {
        // 1) Om användaren klickar "Add address fields" (se cshtml-knappen name="addAddress")
        // Då vill vi bara rendera om sidan med Address-objekt skapat.
        if (addAddress == "true")
        {
            Friend.Address ??= new AddressEditDto();
            return Page();
        }

        // 2) Client-side/server-side model validation (DataAnnotations på DTO + ModelState)
        // Om validering fallerar ska vi rendera om sidan så fel visas vid fälten.
        if (!ModelState.IsValid)
        {
            // Om din vy kräver att Address finns för att rendera fält,
            // kan du skapa den här (men du har gjort vyn robust med if/else).
            // Friend.Address ??= new AddressEditDto();

            return Page();
        }

        // 3) Skicka update till API (FriendId + payload)
        var result = await _api.UpdateFriendAsync(Friend.FriendId, Friend);

        // Vid lyckad uppdatering redirectar vi till Details (PRG-mönster)
        if (result.Ok)
            return RedirectToPage("/Friends/Details/Index", new { friendId = Friend.FriendId });

        // 4) Om API:t svarar med valideringsfel mappar vi dem till ModelState
        // så att felmeddelanden hamnar under rätt input-fält i Razor.
        if (result.ValidationErrors is not null)
        {
            foreach (var (field, messages) in result.ValidationErrors)
            {
                foreach (var msg in messages)
                {
                    // API kan returnera fältnamn som "Email" eller "Address.ZipCode".
                    // I Razor är bindningen "Friend.Email" och "Friend.Address.ZipCode".
                    var key = field.StartsWith("Address.", StringComparison.OrdinalIgnoreCase)
                        ? $"Friend.{field}"   // -> Friend.Address.ZipCode
                        : $"Friend.{field}";  // -> Friend.Email

                    ModelState.AddModelError(key, msg);
                }
            }

            // Om servern har adressfel men modellen saknar Address,
            // skapa Address så fälten kan renderas och visa felmeddelandena.
            var hasAddressErrors = result.ValidationErrors.Keys
                .Any(k => k.StartsWith("Address.", StringComparison.OrdinalIgnoreCase));

            if (hasAddressErrors)
                Friend.Address ??= new AddressEditDto();

            return Page();
        }

        // 5) Övrigt fel (t.ex. generellt felmeddelande från API)
        ErrorMessage = result.Error ?? "Unknown error.";
        return Page();
    }
}

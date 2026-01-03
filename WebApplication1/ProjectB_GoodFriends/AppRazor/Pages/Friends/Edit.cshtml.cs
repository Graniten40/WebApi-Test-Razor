using AppRazor.Models;
using AppRazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages.Friends;

public class EditModel : PageModel
{
    private readonly GoodFriendsApiClient _api;

    public EditModel(GoodFriendsApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public FriendEditDto Friend { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var details = await _api.GetFriendDetailsAsync(id);
        if (details is null)
            return NotFound();

        Friend = new FriendEditDto
        {
            FriendId = details.FriendId,
            FirstName = details.FirstName,
            LastName = details.LastName,
            Email = details.Email,
            Birthday = details.Birthday,
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

    // Stöd för knappen: <button name="addAddress" value="true" ...>
    public async Task<IActionResult> OnPostAsync(string? addAddress)
    {
        // 1) Om användaren klickar "Add address fields"
        if (addAddress == "true")
        {
            Friend.Address ??= new AddressEditDto();
            return Page();
        }

        // 2) Client-side validation (DataAnnotations i AppRazor om du har det)
        if (!ModelState.IsValid)
        {
            // säkerställ att UI inte kraschar om sidan renderas igen
            // (om din cshtml råkar anta att Address finns)
            // Friend.Address ??= new AddressEditDto(); // endast om du alltid vill visa fälten
            return Page();
        }

        // 3) Skicka update till API
        var result = await _api.UpdateFriendAsync(Friend.FriendId, Friend);

        if (result.Ok)
            return RedirectToPage("/Friends/Details/Index", new { friendId = Friend.FriendId });

        // 4) Mappa server-validering till ModelState => visas under rätt fält
        if (result.ValidationErrors is not null)
        {
            foreach (var (field, messages) in result.ValidationErrors)
            {
                foreach (var msg in messages)
                {
                    // field kan komma som:
                    // "Email" eller "FirstName"
                    // eller "Address.ZipCode" / "Address.City" etc.
                    // Vi mappar till Razor-bindings: Friend.Email / Friend.Address.ZipCode
                    var key = field.StartsWith("Address.", StringComparison.OrdinalIgnoreCase)
                        ? $"Friend.{field}"         // -> Friend.Address.ZipCode
                        : $"Friend.{field}";        // -> Friend.Email

                    ModelState.AddModelError(key, msg);
                }
            }

            // Om API säger något om Address.* men Address är null i modellen,
            // skapa den så att fälten kan renderas och visa fel.
            var hasAddressErrors = result.ValidationErrors.Keys
                .Any(k => k.StartsWith("Address.", StringComparison.OrdinalIgnoreCase));

            if (hasAddressErrors)
                Friend.Address ??= new AddressEditDto();

            return Page();
        }

        // 5) Övrigt fel
        ErrorMessage = result.Error ?? "Unknown error.";
        return Page();
    }
}

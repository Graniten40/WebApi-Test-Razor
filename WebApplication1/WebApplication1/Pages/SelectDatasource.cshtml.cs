using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;   // IMusicServiceActive, MusicDataSource

namespace WebApplication1.Pages;

public class SelectDatasourceModel : PageModel
{
    private readonly IMusicServiceActive _musicActive;

    public SelectDatasourceModel(IMusicServiceActive musicActive)
    {
        _musicActive = musicActive;
    }

    [BindProperty]
    public string DataSource { get; set; } = "";

    public void OnGet()
    {
        // Visa nuvarande källa i dropdownen
        DataSource = _musicActive.ActiveDataSource.ToString();
    }

    public IActionResult OnPost()
    {
        if (!Enum.TryParse<MusicDataSource>(DataSource, out var selected))
        {
            ModelState.AddModelError(string.Empty, "Unknown datasource");
            return Page();
        }

        _musicActive.ActiveDataSource = selected;
        TempData["Message"] = $"Datasource changed to {selected}";

        // PRG-pattern: redirect så att F5 inte postar igen
        return RedirectToPage();
    }
}

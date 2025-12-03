using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace WebApplication1.Pages;

public class IndexModel : PageModel
{
    public List<LatinSentence>? LatinSentences { get; set; }

    private readonly ILogger<IndexModel> _logger;
    private readonly LatinService _latinService;

    public IndexModel(ILogger<IndexModel> logger, LatinService latinService)
    {
        _logger = logger;
        _latinService = latinService;
    }

    public void OnGet()
    {
        LatinSentences = _latinService.ReadSentences(0, 10, null);
    }
}

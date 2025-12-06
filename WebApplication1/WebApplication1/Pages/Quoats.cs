using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace MyApp.Namespace;

public class QuatsModel : PageModel   
{
    private readonly IQuoteService _quoteService;

    public List<FamousQuote> Quotes { get; private set; } = [];

    public QuatsModel(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    public void OnGet()
    {
        // Hämta alla quotes (utan paging & filter)
        Quotes = _quoteService.ReadQuotes(null, null, null);
    }
}

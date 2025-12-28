using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppRazor.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GoodFriendsApiClient _api;

    public string? ApiStatus { get; private set; }

    public IndexModel(ILogger<IndexModel> logger, GoodFriendsApiClient api)
    {
        _logger = logger;
        _api = api;
    }

    public async Task OnGetAsync()
    {
        try
        {
            ApiStatus = await _api.GetGuestInfoAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AppWebApi");
            ApiStatus = "API ERROR: " + ex.Message;
        }
    }
}

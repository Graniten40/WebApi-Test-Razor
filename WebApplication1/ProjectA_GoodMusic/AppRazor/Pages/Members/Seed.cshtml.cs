using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace AppRazor.Pages
{
    public class SeedModel : PageModel
    {
        //Just like for WebApi
        readonly IAdminService _admin_service = null;
        readonly ILogger<SeedModel> _logger = null;

        public int NrOfGroups => nrOfGroups().Result;
        private async Task<int> nrOfGroups()
        {
            var info = await _admin_service.GuestInfoAsync();
            return info.Item.Db.NrSeededMusicGroups + info.Item.Db.NrUnseededMusicGroups;
        }

        [BindProperty]
        [Required (ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItemsToSeed { get; set; } = 100;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;

        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (RemoveSeeds)
                {
                    await _admin_service.RemoveSeedAsync(true);
                    await _admin_service.RemoveSeedAsync(false);
                }
                await _admin_service.SeedAsync(NrOfItemsToSeed);

                return Redirect($"~/ListOfGroups");
            }
            return Page();
        }

        //Inject services just like in WebApi
        public SeedModel(IAdminService admin_service, ILogger<SeedModel> logger)
        {
            _admin_service = admin_service;
            _logger = logger;
        }
    }
}

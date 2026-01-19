using Microsoft.Extensions.Logging;
using Models.DTO;
using DbRepos;
using Services.Interfaces;

namespace Services;

public class AdminServiceDb : IAdminService
{
    private readonly AdminDbRepos _repo;
    private readonly ILogger<AdminServiceDb> _logger;

    public AdminServiceDb(AdminDbRepos repo, ILogger<AdminServiceDb> logger)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Simple 1:1 calls in this case, but as Services expands, this will no longer need to be the case
    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync() => _repo.InfoAsync();

    public Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        _logger.LogInformation("SeedAsync called. nrOfItems={NrOfItems}", nrOfItems);
        return _repo.SeedAsync(nrOfItems);
    }

    public Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        _logger.LogInformation("RemoveSeedAsync called. seeded={Seeded}", seeded);
        return _repo.RemoveSeedAsync(seeded);
    }
}

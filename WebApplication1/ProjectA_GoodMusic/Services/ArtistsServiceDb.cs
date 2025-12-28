using Microsoft.Extensions.Logging;

using DbRepos;
using Models.Interfaces;
using Models.DTO;

namespace Services;

public class ArtistsServiceDb : IArtistsService
{
    private readonly ArtistsDbRepos _repo;
    private readonly ILogger<ArtistsServiceDb> _logger;

    #region constructors
    public ArtistsServiceDb(ArtistsDbRepos repo)
    {
        _repo = repo;
    }
    public ArtistsServiceDb(ArtistsDbRepos repo, ILogger<ArtistsServiceDb> logger):this(repo)
    {
        _logger = logger;
    }
    #endregion


    public Task<ResponsePageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadArtistsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<ResponseItemDto<IArtist>> ReadArtistAsync(Guid id, bool flat) => _repo.ReadArtistAsync(id, flat);
    public Task<ResponseItemDto<IArtist>> DeleteArtistAsync(Guid id) => _repo.DeleteArtistAsync(id);
    public Task<ResponseItemDto<IArtist>> UpdateArtistAsync(ArtistCUdto item) => _repo.UpdateArtistAsync(item);
    public Task<ResponseItemDto<IArtist>> CreateArtistAsync(ArtistCUdto item) => _repo.CreateArtistAsync(item);
}


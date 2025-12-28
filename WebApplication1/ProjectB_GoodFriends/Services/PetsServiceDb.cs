using Microsoft.Extensions.Logging;

using Models.Interfaces;
using Models.DTO;
using DbRepos;
using Services.Interfaces;

namespace Services;

public class PetsServiceDb : IPetsService
{
    private readonly PetsDbRepos _repo = null;
    private readonly ILogger<PetsServiceDb> _logger = null;

    public PetsServiceDb(PetsDbRepos repo)
    {
        _repo = repo;
    }
    public PetsServiceDb(PetsDbRepos repo, ILogger<PetsServiceDb> logger):this(repo)
    {
        _logger = logger;
    }
    
    //Simple 1:1 calls in this case, but as Services expands, this will no longer need to be the case
    public Task<ResponsePageDto<IPet>> ReadPetsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadPetsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<ResponseItemDto<IPet>> ReadPetAsync(Guid id, bool flat) => _repo.ReadPetAsync(id, flat);
    public Task<ResponseItemDto<IPet>> DeletePetAsync(Guid id) => _repo.DeletePetAsync(id);
    public Task<ResponseItemDto<IPet>> UpdatePetAsync(PetCuDto item) => _repo.UpdatePetAsync(item);
    public Task<ResponseItemDto<IPet>> CreatePetAsync(PetCuDto item) => _repo.CreatePetAsync(item);
}


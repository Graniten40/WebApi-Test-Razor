using Microsoft.Extensions.Logging;

using Models.Interfaces;
using Models.DTO;
using DbRepos;
using Services.Interfaces;

namespace Services;

public class AddressesServiceDb : IAddressesService
{
    private readonly AddressesDbRepos _repo = null;
    private readonly ILogger<AddressesServiceDb> _logger = null;


    public AddressesServiceDb(AddressesDbRepos repo)
    {
        _repo = repo;
    }
    public AddressesServiceDb(AddressesDbRepos repo, ILogger<AddressesServiceDb> logger):this(repo)
    {
        _logger = logger;
    }
    
    //Simple 1:1 calls in this case, but as Services expands, this will no longer need to be the case
    public Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadAddressesAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<ResponseItemDto<IAddress>> ReadAddressAsync(Guid id, bool flat) => _repo.ReadAddressAsync(id, flat);
    public Task<ResponseItemDto<IAddress>> DeleteAddressAsync(Guid id) => _repo.DeleteAddressAsync(id);
    public Task<ResponseItemDto<IAddress>> UpdateAddressAsync(AddressCuDto item) => _repo.UpdateAddressAsync(item);
    public Task<ResponseItemDto<IAddress>> CreateAddressAsync(AddressCuDto item) => _repo.CreateAddressAsync(item);
}


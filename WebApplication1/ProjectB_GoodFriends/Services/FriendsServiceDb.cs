using Microsoft.Extensions.Logging;

using Models.Interfaces;
using Models.DTO;
using DbRepos;
using Services.Interfaces;

namespace Services;

public class FriendsServiceDb : IFriendsService
{
    private readonly FriendsDbRepos _repo = null;
    private readonly ILogger<FriendsServiceDb> _logger = null;

    public FriendsServiceDb(FriendsDbRepos repo)
    {
        _repo = repo;
    }
    public FriendsServiceDb(FriendsDbRepos repo, ILogger<FriendsServiceDb> logger) : this(repo)
    {
        _logger = logger;
    }

    //Simple 1:1 calls in this case, but as Services expands, this will no longer need to be the case
    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadFriendsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<ResponseItemDto<IFriend>> ReadFriendAsync(Guid id, bool flat) => _repo.ReadFriendAsync(id, flat);
    public Task<ResponseItemDto<IFriend>> DeleteFriendAsync(Guid id) => _repo.DeleteFriendAsync(id);
    public Task<ResponseItemDto<IFriend>> UpdateFriendAsync(FriendCuDto item) => _repo.UpdateFriendAsync(item);
    public Task<ResponseItemDto<IFriend>> CreateFriendAsync(FriendCuDto item) => _repo.CreateFriendAsync(item);
}


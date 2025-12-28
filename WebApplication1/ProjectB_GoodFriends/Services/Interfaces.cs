using Models.Interfaces;
using Models.DTO;

namespace Services.Interfaces;

public interface IAdminService
{
    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync();
    public Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems);
    public Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded);
}

public interface IFriendsService
{
    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IFriend>> ReadFriendAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IFriend>> DeleteFriendAsync(Guid id);
    public Task<ResponseItemDto<IFriend>> UpdateFriendAsync(FriendCuDto item);
    public Task<ResponseItemDto<IFriend>> CreateFriendAsync(FriendCuDto item);
}

public interface IAddressesService
{
    public Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IAddress>> ReadAddressAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IAddress>> DeleteAddressAsync(Guid id);
    public Task<ResponseItemDto<IAddress>> UpdateAddressAsync(AddressCuDto item);
    public Task<ResponseItemDto<IAddress>> CreateAddressAsync(AddressCuDto item);
}

public interface IPetsService
{
    public Task<ResponsePageDto<IPet>> ReadPetsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IPet>> ReadPetAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IPet>> DeletePetAsync(Guid id);
    public Task<ResponseItemDto<IPet>> UpdatePetAsync(PetCuDto item);
    public Task<ResponseItemDto<IPet>> CreatePetAsync(PetCuDto item);
}

public interface IQuotesService
{
    public Task<ResponsePageDto<IQuote>> ReadQuotesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IQuote>> ReadQuoteAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IQuote>> DeleteQuoteAsync(Guid id);
    public Task<ResponseItemDto<IQuote>> UpdateQuoteAsync(QuoteCuDto item);
    public Task<ResponseItemDto<IQuote>> CreateQuoteAsync(QuoteCuDto item);
}

public interface ILoginService
{
    public Task<ResponseItemDto<LoginUserSessionDto>> LoginUserAsync(LoginCredentialsDto usrCreds);
}
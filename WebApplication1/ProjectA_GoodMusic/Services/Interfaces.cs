using Models.Interfaces;
using Models.DTO;

namespace Services;

public interface IAdminService
{
    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync();
    public Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems);
    public Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded);
}
public interface IMusicGroupsService
{
    public Task<ResponsePageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IMusicGroup>> ReadMusicGroupAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IMusicGroup>> DeleteMusicGroupAsync(Guid id);
    public Task<ResponseItemDto<IMusicGroup>> UpdateMusicGroupAsync(MusicGroupCUdto item);
    public Task<ResponseItemDto<IMusicGroup>> CreateMusicGroupAsync(MusicGroupCUdto item);
}

public interface IAlbumsService
{
    public Task<ResponsePageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IAlbum>> ReadAlbumAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IAlbum>> DeleteAlbumAsync(Guid id);
    public Task<ResponseItemDto<IAlbum>> UpdateAlbumAsync(AlbumCUdto item);
    public Task<ResponseItemDto<IAlbum>> CreateAlbumAsync(AlbumCUdto item);
}

public interface IArtistsService
{
    public Task<ResponsePageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IArtist>> ReadArtistAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IArtist>> DeleteArtistAsync(Guid id);
    public Task<ResponseItemDto<IArtist>> UpdateArtistAsync(ArtistCUdto item);
    public Task<ResponseItemDto<IArtist>> CreateArtistAsync(ArtistCUdto item);
}

public interface ILoginService
{
    public Task<ResponseItemDto<LoginUserSessionDto>> LoginUserAsync(LoginCredentialsDto usrCreds);
}


public enum MusicDataSource { SQLDatabase, WebApi }
public interface IMusicServiceActive
{
    public MusicDataSource ActiveDataSource {get; set;}
}


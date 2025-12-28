using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Data;

using Models.DTO;
using DbModels;
using DbContext;
using Models.Interfaces;

namespace DbRepos;

public class ArtistsDbRepos
{
    private ILogger<ArtistsDbRepos> _logger;
    private readonly MainDbContext _dbContext;

    public ArtistsDbRepos(ILogger<ArtistsDbRepos> logger, MainDbContext context)
    {
        _logger = logger;
        _dbContext = context;
    }

    public async Task<ResponseItemDto<IArtist>> ReadArtistAsync(Guid id, bool flat)
    {
        IArtist item;
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Artists.AsNoTracking()
                .Include(a => a.MusicGroupsDbM)
                .ThenInclude(a => a.AlbumsDbM)
                .Where(i => i.ArtistId == id);

            item = await query.FirstOrDefaultAsync<IArtist>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Artists.AsNoTracking()
                .Where(i => i.ArtistId == id);

            item = await query.FirstOrDefaultAsync<IArtist>();
        }
        
        if (item == null) throw new ArgumentException($"Item {id} is not existing");
        return new ResponseItemDto<IArtist>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            Item = item
        };
    }

    public async Task<ResponsePageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<ArtistDbM> query;
        if (flat)
        {
            query = _dbContext.Artists.AsNoTracking();
        }
        else
        {
            query = _dbContext.Artists.AsNoTracking()
                .Include(a => a.MusicGroupsDbM)
                .ThenInclude(a => a.AlbumsDbM);
        }

        var ret = new ResponsePageDto<IArtist>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) &&
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) &&
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IArtist>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<ResponseItemDto<IArtist>> DeleteArtistAsync(Guid id)
    {
        var query1 = _dbContext.Artists
            .Where(i => i.ArtistId == id);

        var item = await query1.FirstOrDefaultAsync<ArtistDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Artists.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return new ResponseItemDto<IArtist>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            Item = item
        };
    }

    public async Task<ResponseItemDto<IArtist>> UpdateArtistAsync(ArtistCUdto itemDto)
    {
        //Find the instance with matching id and read the navigation properties.
        var query1 = _dbContext.Artists
            .Where(i => i.ArtistId == itemDto.ArtistId);
        var item = await query1
                .Include(i => i.MusicGroupsDbM)
                .FirstOrDefaultAsync();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.ArtistId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_ArtistCUdto_to_ArtistDbM(itemDto, item);

        //write to database model
        _dbContext.Artists.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadArtistAsync(item.ArtistId, false);
    }

    public async Task<ResponseItemDto<IArtist>> CreateArtistAsync(ArtistCUdto itemDto)
    {
        if (itemDto.ArtistId != null)
            throw new ArgumentException($"{nameof(itemDto.ArtistId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties. Seeded always false on created items
        itemDto.Seeded = false;
        var item = new ArtistDbM(itemDto);

        //Update navigation properties
        await navProp_ArtistCUdto_to_ArtistDbM(itemDto, item);


        //write to database model
        _dbContext.Artists.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadArtistAsync(item.ArtistId, false);
    }


    //from all Guid relationships in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst 
    //as navigation properties. Error is thrown if no object is found corresponing to an id.
    private  async Task navProp_ArtistCUdto_to_ArtistDbM(ArtistCUdto itemDtoSrc, ArtistDbM itemDst)
    {
        //Navigation prop MusicGroups
        List<MusicGroupDbM> mgs = new List<MusicGroupDbM>();
        foreach (var id in itemDtoSrc.MusicGroupsId)
        {
            var musicGroup = await _dbContext.MusicGroups.FirstOrDefaultAsync(a => a.MusicGroupId == id);

            if (musicGroup == null)
                throw new ArgumentException($"Item id {id} not existing");

            mgs.Add(musicGroup);
        }

        itemDst.MusicGroupsDbM = mgs;
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Data;

using Models.Interfaces;
using Models.DTO;
using DbModels;
using DbContext;

namespace DbRepos;

public class AddressesDbRepos
{
    private ILogger<AddressesDbRepos> _logger;
    private readonly MainDbContext _dbContext;

    public AddressesDbRepos(ILogger<AddressesDbRepos> logger, MainDbContext context)
    {
        _logger = logger;
        _dbContext = context;
    }

    public async Task<ResponseItemDto<IAddress>> ReadAddressAsync(Guid id, bool flat)
    {
        IAddress item;
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Addresses.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.QuotesDbM)
                .Where(i => i.AddressId == id);

            item = await query.FirstOrDefaultAsync<IAddress>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Addresses.AsNoTracking()
                .Where(i => i.AddressId == id);
                
            item = await query.FirstOrDefaultAsync<IAddress>();
        }

        if (item == null) throw new ArgumentException($"Item {id} is not existing");
        return new ResponseItemDto<IAddress>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            Item = item
        };
    }

    public async Task<ResponsePageDto<IAddress>> ReadAddressesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<AddressDbM> query;
        if (flat)
        {
            query = _dbContext.Addresses.AsNoTracking();
        }
        else
        {
            query = _dbContext.Addresses.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.QuotesDbM);
        }

        var ret = new ResponsePageDto<IAddress>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.StreetAddress.ToLower().Contains(filter) ||
                            i.City.ToLower().Contains(filter) ||
                            i.Country.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.StreetAddress.ToLower().Contains(filter) ||
                            i.City.ToLower().Contains(filter) ||
                            i.Country.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IAddress>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<ResponseItemDto<IAddress>> DeleteAddressAsync(Guid id)
    {
        var query1 = _dbContext.Addresses
            .Where(i => i.AddressId == id);

        var item = await query1.FirstOrDefaultAsync<AddressDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Addresses.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return new ResponseItemDto<IAddress>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif

            Item = item
        };
    }

    public async Task<ResponseItemDto<IAddress>> UpdateAddressAsync(AddressCuDto itemDto)
    {
        var query1 = _dbContext.Addresses
            .Where(i => i.AddressId == itemDto.AddressId);
        var item = await query1
                .Include(i => i.FriendsDbM)
                .FirstOrDefaultAsync<AddressDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.AddressId} is not existing");

        //I cannot have duplicates in the Addresses table, so check that
        var query2 = _dbContext.Addresses
            .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
            (i.ZipCode == itemDto.ZipCode) &&
            (i.City == itemDto.City) &&
            (i.Country == itemDto.Country)));
        var existingItem = await query2.FirstOrDefaultAsync<AddressDbM>();
        if (existingItem != null && existingItem.AddressId != itemDto.AddressId)
            throw new ArgumentException($"Item already exist with id {existingItem.AddressId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_AddressCUdto_to_AddressDbM(itemDto, item);

        //write to database model
        _dbContext.Addresses.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAddressAsync(item.AddressId, false);    
    }

    public async Task<ResponseItemDto<IAddress>> CreateAddressAsync(AddressCuDto itemDto)
    { 
        if (itemDto.AddressId != null)
            throw new ArgumentException($"{nameof(itemDto.AddressId)} must be null when creating a new object");

        //I cannot have duplicates in the Addresses table, so check that
        var query2 = _dbContext.Addresses
            .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
                (i.ZipCode == itemDto.ZipCode) &&
                (i.City == itemDto.City) &&
                (i.Country == itemDto.Country)));
        var existingItem = await query2.FirstOrDefaultAsync<AddressDbM>();
        if (existingItem != null)
            throw new ArgumentException($"Item already exist with id {existingItem.AddressId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        var item = new AddressDbM(itemDto);

        //Update navigation properties
        await navProp_AddressCUdto_to_AddressDbM(itemDto, item);

        //write to database model
        _dbContext.Addresses.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAddressAsync(item.AddressId, false);    
    }

    private async Task navProp_AddressCUdto_to_AddressDbM(AddressCuDto itemDtoSrc, AddressDbM itemDst)
    {
        //update FriendsDbM from itemDto.FriendId
        List<FriendDbM> friends = null;
        if (itemDtoSrc.FriendsId != null)
        {
            friends = new List<FriendDbM>();
            foreach (var id in itemDtoSrc.FriendsId)
            {
                var f = await _dbContext.Friends.FirstOrDefaultAsync(i => i.FriendId == id);
                if (f == null)
                    throw new ArgumentException($"Item id {id} not existing");

                friends.Add(f);
            }
        }
        itemDst.FriendsDbM = friends;
    }
}

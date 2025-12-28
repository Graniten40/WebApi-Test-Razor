using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Data;

using Models.Interfaces;
using Models.DTO;
using DbModels;
using DbContext;

namespace DbRepos;

public class QuotesDbRepos
{
    private ILogger<QuotesDbRepos> _logger;
    private readonly MainDbContext _dbContext;

    public QuotesDbRepos(ILogger<QuotesDbRepos> logger, MainDbContext context)
    {
        _logger = logger;
        _dbContext = context;
    }

    public async Task<ResponseItemDto<IQuote>> ReadQuoteAsync(Guid id, bool flat)
    {
        IQuote item;
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Quotes.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.AddressDbM)
                .Where(i => i.QuoteId == id);

            item = await query.FirstOrDefaultAsync<IQuote>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Quotes.AsNoTracking()
                .Where(i => i.QuoteId == id);

            item = await query.FirstOrDefaultAsync<IQuote>();
        }
        
        if (item == null) throw new ArgumentException($"Item {id} is not existing");
        return new ResponseItemDto<IQuote>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            Item = item
        };
    }

    public async Task<ResponsePageDto<IQuote>> ReadQuotesAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<QuoteDbM> query;
        if (flat)
        {
            query = _dbContext.Quotes.AsNoTracking();
        }
        else
        {
            query = _dbContext.Quotes.AsNoTracking()
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.PetsDbM)
                .Include(i => i.FriendsDbM)
                .ThenInclude(i => i.AddressDbM);

        }

        var ret = new ResponsePageDto<IQuote>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.QuoteText.ToLower().Contains(filter) ||
                            i.Author.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.QuoteText.ToLower().Contains(filter) ||
                            i.Author.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IQuote>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret; 
    }

    public async Task<ResponseItemDto<IQuote>> DeleteQuoteAsync(Guid id)
    {
        var query1 = _dbContext.Quotes
            .Where(i => i.QuoteId == id);

        var item = await query1.FirstOrDefaultAsync<QuoteDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Quotes.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        return new ResponseItemDto<IQuote>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            Item = item
        };
    }

    public async Task<ResponseItemDto<IQuote>> UpdateQuoteAsync(QuoteCuDto itemDto)
    {
        var query1 = _dbContext.Quotes
            .Where(i => i.QuoteId == itemDto.QuoteId);
        var item = await query1
                .Include(i => i.FriendsDbM)
                .FirstOrDefaultAsync<QuoteDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.QuoteId} is not existing");

        //Avoid duplicates in the Quotes table, so check that
        var query2 = _dbContext.Quotes
            .Where(i => ((i.Author == itemDto.Author) &&
            (i.QuoteText == itemDto.Quote)));
        var existingItem = await query2.FirstOrDefaultAsync<QuoteDbM>();
        if (existingItem != null && existingItem.QuoteId != itemDto.QuoteId)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");


        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);

        //write to database model
        _dbContext.Quotes.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadQuoteAsync(item.QuoteId, false);    
 }

    public async Task<ResponseItemDto<IQuote>> CreateQuoteAsync(QuoteCuDto itemDto)
    {
        if (itemDto.QuoteId != null)
            throw new ArgumentException($"{nameof(itemDto.QuoteId)} must be null when creating a new object");

        //Avoid duplicates in the Quotes table, so check that
        var query2 = _dbContext.Quotes
            .Where(i => ((i.Author == itemDto.Author) &&
            (i.QuoteText == itemDto.Quote)));
        var existingItem = await query2.FirstOrDefaultAsync<QuoteDbM>();
        if (existingItem != null)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        var item = new QuoteDbM(itemDto);

        //Update navigation properties
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);


        //write to database model
        _dbContext.Quotes.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadQuoteAsync(item.QuoteId, false);    
    }

    private async Task navProp_QuoteCUdto_to_QuoteDbM(QuoteCuDto itemDtoSrc, QuoteDbM itemDst)
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

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Models.Interfaces;
using Models.DTO;
using DbModels;
using DbContext;

namespace DbRepos;

public class QuotesDbRepos
{
    private readonly ILogger<QuotesDbRepos> _logger;
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
            // Fullt laddad (Quote -> Friends -> Pets/Address)
            var query = _dbContext.Quotes.AsNoTracking()
                .Include(q => q.FriendsDbM)
                    .ThenInclude(f => f.PetsDbM)
                .Include(q => q.FriendsDbM)
                    .ThenInclude(f => f.AddressDbM)
                .Where(q => q.QuoteId == id);

            item = await query.FirstOrDefaultAsync<IQuote>();
        }
        else
        {
            // Flat: endast quote
            var query = _dbContext.Quotes.AsNoTracking()
                .Where(q => q.QuoteId == id);

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
            // Fullt laddad: Quotes -> Friends -> Pets/Address
            query = _dbContext.Quotes.AsNoTracking()
                .Include(q => q.FriendsDbM)
                    .ThenInclude(f => f.PetsDbM)
                .Include(q => q.FriendsDbM)
                    .ThenInclude(f => f.AddressDbM);
        }

        // (Små förbättringar: undvik null-problem + ToLower på null)
        var filtered = query.Where(q =>
            (q.Seeded == seeded) &&
            (
                (q.QuoteText ?? "").ToLower().Contains(filter) ||
                (q.Author ?? "").ToLower().Contains(filter)
            )
        );

        var ret = new ResponsePageDto<IQuote>()
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif
            DbItemsCount = await filtered.CountAsync(),

            PageItems = await filtered
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
        var item = await _dbContext.Quotes
            .Where(q => q.QuoteId == id)
            .FirstOrDefaultAsync<QuoteDbM>();

        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        _dbContext.Quotes.Remove(item);
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
        var item = await _dbContext.Quotes
            .Where(q => q.QuoteId == itemDto.QuoteId)
            .Include(q => q.FriendsDbM) // viktigt: vi uppdaterar join-relationer
            .FirstOrDefaultAsync<QuoteDbM>();

        if (item == null) throw new ArgumentException($"Item {itemDto.QuoteId} is not existing");

        // Avoid duplicates in Quotes
        var dtoText = (itemDto.QuoteText ?? "").Trim();
        var dtoAuthor = (itemDto.Author ?? "").Trim();

        var existingItem = await _dbContext.Quotes
            .Where(q => (q.Author == dtoAuthor) && (q.QuoteText == dtoText))
            .FirstOrDefaultAsync<QuoteDbM>();


        if (existingItem != null && existingItem.QuoteId != itemDto.QuoteId)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");

        // Update scalar props
        item.UpdateFromDTO(itemDto);

        // Update navigation (join table)
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);

        await _dbContext.SaveChangesAsync();

        return await ReadQuoteAsync(item.QuoteId, false);
    }

    public async Task<ResponseItemDto<IQuote>> CreateQuoteAsync(QuoteCuDto itemDto)
    {
        if (itemDto.QuoteId != null)
            throw new ArgumentException($"{nameof(itemDto.QuoteId)} must be null when creating a new object");

        // Avoid duplicates
        var dtoText = (itemDto.QuoteText ?? "").Trim();
        var dtoAuthor = (itemDto.Author ?? "").Trim();

        var existingItem = await _dbContext.Quotes
            .Where(q => (q.Author == dtoAuthor) && (q.QuoteText == dtoText))
            .FirstOrDefaultAsync<QuoteDbM>();


        if (existingItem != null)
            throw new ArgumentException($"Item already exist with id {existingItem.QuoteId}");

        var item = new QuoteDbM(itemDto);

        // koppla relation(er) via join-tabellen
        await navProp_QuoteCUdto_to_QuoteDbM(itemDto, item);

        _dbContext.Quotes.Add(item);
        await _dbContext.SaveChangesAsync();

        return await ReadQuoteAsync(item.QuoteId, false);
    }

    private async Task navProp_QuoteCUdto_to_QuoteDbM(QuoteCuDto itemDtoSrc, QuoteDbM itemDst)
    {
        // DTO har FriendsId (lista). Vi stödjer 1..n ids.
        if (itemDtoSrc.FriendsId == null || itemDtoSrc.FriendsId.Count == 0)
            throw new ArgumentException("FriendsId must contain at least one id for a Quote");

        var ids = itemDtoSrc.FriendsId
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new ArgumentException("FriendsId must contain valid ids for a Quote");

        // Ladda alla friends som ska kopplas
        var friends = await _dbContext.Friends
            .Where(f => ids.Contains(f.FriendId))
            .ToListAsync();

        if (friends.Count != ids.Count)
        {
            var found = friends.Select(f => f.FriendId).ToHashSet();
            var missing = ids.Where(id => !found.Contains(id));
            throw new ArgumentException($"Friend id(s) not existing: {string.Join(", ", missing)}");
        }

        // Uppdatera join-relationen (clear + add)
        itemDst.FriendsDbM.Clear();
        foreach (var f in friends)
            itemDst.FriendsDbM.Add(f);
    }
}

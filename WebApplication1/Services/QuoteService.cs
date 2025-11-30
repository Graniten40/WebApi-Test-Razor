using Models;
using System.Linq;
using Seido.Utilities.SeedGenerator;

namespace Services;


public class QuoteService : IQuoteService
{
    readonly List<FamousQuote> _quotes = new SeedGenerator().AllQuotes.Select(q => new FamousQuote(){Author = q.Author, Quote = q.Quote}).ToList();

    #region CRUD 
    public int NrOfQuotes(string filter)
    {
        filter ??= "";
        return _quotes.Count(q =>
            q.Quote != null && q.Quote.ToLower().Contains(filter.ToLower()) ||
            q.Author != null && q.Author.ToLower().Contains(filter.ToLower()));
    }

    public List<FamousQuote> ReadQuotes(int? pageNumber, int? pageSize, string filter)
    {
        filter ??= "";
        if (pageNumber == null || pageSize == null)
        {
            return _quotes.Where(q =>
                                q.Quote != null && q.Quote.ToLower().Contains(filter.ToLower()) ||
                                q.Author != null && q.Author.ToLower().Contains(filter.ToLower()))
                          .Select(q => new FamousQuote(q)).ToList();
        }

        return _quotes.Where(q =>
                                q.Quote != null && q.Quote.ToLower().Contains(filter.ToLower()) ||
                                q.Author != null && q.Author.ToLower().Contains(filter.ToLower()))
                          .Select(q => new FamousQuote(q))

            //Adding paging
            .Skip(pageNumber.Value * pageSize.Value)
            .Take(pageSize.Value).ToList();
    }


    public FamousQuote ReadQuote(Guid id)
    {
        var q = _quotes.FirstOrDefault(q => q.QuoteId == id);
        if (q != null)
        {
            return new FamousQuote(q);
        }
        
        return null;
    }

    public FamousQuote UpdateQuote(FamousQuote _src)
    {
        var q = _quotes.FirstOrDefault(q => q.QuoteId == _src.QuoteId);
        if (q != null)
        {
            q.Quote = _src.Quote;
            q.Author = _src.Author;
            return new FamousQuote(q);
        }
        return null;
    }

    public FamousQuote CreateQuote(FamousQuote _src)
    {
       _quotes.Add(_src); 
       return new FamousQuote(_src);
    }

    public FamousQuote DeleteQuote(Guid id)
    {
        var q = _quotes.FirstOrDefault(q => q.QuoteId == id);

        //If the item does not exists
        if (q == null) throw new ArgumentException($"Item {id} is not existing");

        _quotes.Remove(q);
        return q;
    }

    #endregion
}




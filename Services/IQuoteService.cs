using System;
using Models;

namespace Services
{
    public interface IQuoteService
    {
        public int NrOfQuotes(string filter = null);

        public List<FamousQuote> ReadQuotes(int? pageNumber = null, int? pageSize = null, string filter = null);
        public FamousQuote ReadQuote(Guid id);

        public FamousQuote UpdateQuote(FamousQuote _src);

        public FamousQuote CreateQuote(FamousQuote _src);

        public FamousQuote DeleteQuote(Guid id);
    }
}
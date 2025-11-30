using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Linq;

using Configuration;

namespace Models
{
    public class FamousQuote
    {
        public Guid QuoteId {get; set;} = Guid.NewGuid();
        public string Quote { get; set; }
        public string Author { get; set; }

        public FamousQuote() {}
        public FamousQuote(FamousQuote original)
        {
            QuoteId = original.QuoteId;
            Quote = original.Quote;
            Author = original.Author;
        }
    }
}
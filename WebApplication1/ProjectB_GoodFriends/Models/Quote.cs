using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;

public class Quote : IQuote, ISeed<Quote>, IEquatable<Quote>
{
    public virtual Guid QuoteId { get; set; }
    public virtual string QuoteText { get; set; }
    public virtual string Author { get; set; }

    // Model relationships
    // One Quote may have many friends
    public virtual List<IFriend> Friends { get; set; } = null;


    #region constructors
    public Quote() { }

    public Quote(SeededQuote goodQuote)
    {
        QuoteId = Guid.NewGuid();
        QuoteText = goodQuote.Quote;
        Author = goodQuote.Author;
        Seeded = true;
    }

    #endregion

    #region implementing IEquatable

    public bool Equals(Quote other) => (other != null) && ((QuoteText, Author) == (other.QuoteText, other.Author));
    public override bool Equals(object obj) => Equals(obj as Quote);
    public override int GetHashCode() => (QuoteText, Author).GetHashCode();

    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual Quote Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;
        QuoteId = Guid.NewGuid();

        var quote = seedGenerator.Quote;
        QuoteText = quote.Quote;
        Author = quote.Author;

        return this;
    }
    #endregion
}



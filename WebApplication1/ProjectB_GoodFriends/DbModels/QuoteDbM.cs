using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;
using Models.Interfaces;
using Models.DTO;

namespace DbModels;

[Table("Quotes", Schema = "supusr")]
public sealed class QuoteDbM : Quote, ISeed<QuoteDbM>, IEquatable<QuoteDbM>
{
    [Key]
    public override Guid QuoteId { get; set; }

    // Many-to-many: Quotes har INTE FriendId i DB.
    // Kopplingen sker via join-tabellen FriendDbMQuoteDbM
    [JsonIgnore]
    public ICollection<FriendDbM> FriendsDbM { get; set; } = new List<FriendDbM>();

    #region Interface-based navigation (NOT mapped)
    // Basmodellen använder Friends (List<IFriend>).
    // Här mappar vi den mot DbModel-navigationen FriendsDbM.
    [NotMapped]
    public override List<IFriend> Friends
    {
        get => FriendsDbM?.Cast<IFriend>().ToList() ?? new List<IFriend>();
        set => throw new NotImplementedException("Set relationship via join-table (FriendsDbM collection).");
    }
    #endregion

    #region constructors
    public QuoteDbM() : base() { }

    // VIKTIGT: Anropa INTE base(goodQuote) här, för base-ctorn sätter Friends
    // och det kan trigga NotImplementedException i vår override (setter).
    public QuoteDbM(SeededQuote goodQuote) : base()
    {
        QuoteId = Guid.NewGuid();
        QuoteText = goodQuote.Quote;
        Author = goodQuote.Author;
        Seeded = true;

        // Sätt INTE Friends här.
        // Relation kopplas via join-tabellen FriendDbMQuoteDbM (t.ex. i AdminDbRepos).
    }

    public QuoteDbM(QuoteCuDto org) : base()
    {
        QuoteId = Guid.NewGuid();
        UpdateFromDTO(org);
    }
    #endregion

    #region implementing IEquatable
    public bool Equals(QuoteDbM? other) =>
        other != null && (QuoteText, Author) == (other.QuoteText, other.Author);

    public override bool Equals(object? obj) => Equals(obj as QuoteDbM);
    public override int GetHashCode() => (QuoteText, Author).GetHashCode();
    #endregion

    #region randomly seed this instance
    public override QuoteDbM Seed(SeedGenerator sgen)
    {
        // base.Seed(sgen) är OK så länge base.Seed inte gör Friends = ...
        // Din base.Seed använder: Friends ??= new List<IFriend>(); (det är OK)
        base.Seed(sgen);
        return this;
    }
    #endregion

    #region Update from DTO
    public Quote UpdateFromDTO(QuoteCuDto org)
    {
        if (org == null) return null!;

        Author = org.Author;

        // FIX: Quote -> QuoteText
        QuoteText = org.QuoteText;

        return this;
    }
    #endregion
}

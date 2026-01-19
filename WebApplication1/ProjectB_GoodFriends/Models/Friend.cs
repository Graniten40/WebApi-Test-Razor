using System;
using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;

public class csFriend : IFriend, ISeed<csFriend>
{
    public virtual Guid FriendId { get; set; }

    public virtual string FirstName { get; set; } = string.Empty;
    public virtual string LastName { get; set; } = string.Empty;

    public virtual string Email { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; } = null;

    // Navigationer finns kvar i modellen – men seedas INTE här
    public virtual IAddress? Address { get; set; } = null;
    public virtual System.Collections.Generic.List<IPet> Pets { get; set; } = new();
    public virtual System.Collections.Generic.List<IQuote> Quotes { get; set; } = new();

    public csFriend() { }

    public csFriend(csFriend org)
    {
        if (org is null) throw new ArgumentNullException(nameof(org));

        Seeded = org.Seeded;
        FriendId = org.FriendId;
        FirstName = org.FirstName ?? string.Empty;
        LastName = org.LastName ?? string.Empty;
        Email = org.Email ?? string.Empty;
        Birthday = org.Birthday;

        // Kopiera relationer “för domänbruk” (inte nödvändigt för seed)
        Address = org.Address;
        Pets = org.Pets ?? new();
        Quotes = org.Quotes ?? new();
    }

    public bool Seeded { get; set; } = false;

    // OBS: Scalar-only seed. Navigationer seedas i DbModels (FriendDbM)
    public virtual csFriend Seed(SeedGenerator sgen)
    {
        if (sgen is null) throw new ArgumentNullException(nameof(sgen));

        Seeded = true;
        FriendId = Guid.NewGuid();

        FirstName = sgen.FirstName ?? string.Empty;
        LastName  = sgen.LastName ?? string.Empty;
        Email     = sgen.Email(FirstName, LastName) ?? string.Empty;

        Birthday = sgen.Bool ? sgen.DateAndTime(1970, 2000) : null;

        // Viktigt: gör inget med Address/Pets/Quotes här.
        return this;
    }
}

using System.ComponentModel.DataAnnotations.Schema;
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

    // FK (bra för EF + relationsmodell)
    public virtual Guid? AddressId { get; set; } = null;

    // EF navigation: concrete type
    public virtual Address? Address { get; set; } = null;

    // Interface navigation: keep contract IAddress?
    IAddress? IFriend.Address
    {
        get => Address;
        set => Address = value as Address;
    }

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

        AddressId = org.AddressId;
        Address = org.Address;

        Pets = org.Pets ?? new();
        Quotes = org.Quotes ?? new();
    }

    public bool Seeded { get; set; } = false;

    public virtual csFriend Seed(SeedGenerator sgen)
    {
        if (sgen is null) throw new ArgumentNullException(nameof(sgen));

        Seeded = true;
        FriendId = Guid.NewGuid();

        FirstName = sgen.FirstName ?? string.Empty;
        LastName = sgen.LastName ?? string.Empty;
        Email = sgen.Email(FirstName, LastName) ?? string.Empty;

        Birthday = sgen.Bool ? sgen.DateAndTime(1970, 2000) : null;

        return this;
    }
}

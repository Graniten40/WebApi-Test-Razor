using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;

public class csFriend : IFriend, ISeed<csFriend>
{
    public virtual Guid FriendId { get; set; }

    public virtual string FirstName { get; set; }
    public virtual string LastName { get; set; }

    public virtual string Email { get; set; }
    public DateTime? Birthday { get; set; } = null;

    // Model relationships
    // One Friend may only have one address
    public virtual IAddress Address { get; set; } = null;

    // One Friend may have many favorite pets
    public virtual List<IPet> Pets { get; set; } = null;

    // One Friend may have many favorite quotes
    public virtual List<IQuote> Quotes { get; set; } = null;


    #region contructors
    public csFriend() { }

    public csFriend(csFriend org)
    {
        this.Seeded = org.Seeded;

        this.FriendId = org.FriendId;
        this.FirstName = org.FirstName;
        this.LastName = org.LastName;
        this.Email = org.Email;

        //use the ternary operator to create only if the orginal is not null
        this.Address = (org.Address != null)? new Address((Address)org.Address): null;

        //using Linq Select and copy contructor to create a list copy
        this.Pets = (org.Pets != null) ? org.Pets.Select(p => new Pet((Pet) p)).ToList<IPet>() : null;
    }
    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual csFriend Seed(SeedGenerator sgen)
    {
        Seeded = true;
        FriendId = Guid.NewGuid();
        FirstName = sgen.FirstName;
        LastName = sgen.LastName;
        Email = sgen.Email(FirstName, LastName);
        Birthday = (sgen.Bool) ? sgen.DateAndTime(1970, 2000) : null;

        return this;
    }
    #endregion
}


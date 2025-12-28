namespace Models.Interfaces;

public interface IFriend
{
    public Guid FriendId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string Email { get; set; }
    public IAddress Address { get; set; }
    public DateTime? Birthday { get; set; }

    public List<IPet> Pets { get; set; }
    public List<IQuote> Quotes { get; set; }
}

public interface IAddress
{
    public Guid AddressId { get; set; }

    public string StreetAddress { get; set; }
    public int ZipCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public List<IFriend> Friends { get; set; }
}

public enum AnimalKind { Dog, Cat, Rabbit, Fish, Bird };
public enum AnimalMood { Happy, Hungry, Lazy, Sulky, Buzy, Sleepy };
public interface IPet
{
    public Guid PetId { get; set; }

    public AnimalKind Kind { get; set; }
    public AnimalMood Mood { get; set; }
    public string Name { get; set; }

    public IFriend Friend { get; set; }
}

public interface IQuote
{
    public Guid QuoteId { get; set; }

    public string QuoteText { get; set; }
    public string Author { get; set; }

    public List<IFriend> Friends { get; set; }
}

public interface IUser
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public string UserRole { get; set; }
}
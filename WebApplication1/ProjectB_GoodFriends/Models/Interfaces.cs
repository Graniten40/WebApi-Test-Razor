namespace Models.Interfaces;

public interface IFriend
{
    Guid FriendId { get; set; }

    string FirstName { get; set; }
    string LastName { get; set; }

    string Email { get; set; }

    // A friend may or may not have an address
    IAddress? Address { get; set; }

    DateTime? Birthday { get; set; }

    // Collections should never be null in practice (use empty lists)
    List<IPet> Pets { get; set; }
    List<IQuote> Quotes { get; set; }
}

public interface IAddress
{
    Guid AddressId { get; set; }

    string StreetAddress { get; set; }
    int ZipCode { get; set; }
    string City { get; set; }
    string Country { get; set; }

    // Collections should not be null
    List<IFriend> Friends { get; set; }
}

public enum AnimalKind { Dog, Cat, Rabbit, Fish, Bird }
public enum AnimalMood { Happy, Hungry, Lazy, Sulky, Buzy, Sleepy }

public interface IPet
{
    Guid PetId { get; set; }

    AnimalKind Kind { get; set; }
    AnimalMood Mood { get; set; }

    string Name { get; set; }

    // A pet may exist before it is linked to a friend
    IFriend? Friend { get; set; }
}

public interface IQuote
{
    Guid QuoteId { get; set; }

    string QuoteText { get; set; }
    string Author { get; set; }

    // Collections should not be null
    List<IFriend> Friends { get; set; }
}

public interface IUser
{
    Guid UserId { get; set; }

    string UserName { get; set; }
    string Email { get; set; }
    string Password { get; set; }

    string UserRole { get; set; }
}

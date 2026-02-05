using System.ComponentModel.DataAnnotations.Schema;
using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;

public class Address : IAddress, ISeed<Address>, IEquatable<Address>
{
    public virtual Guid AddressId { get; set; }

    public virtual string StreetAddress { get; set; } = string.Empty;
    public virtual int ZipCode { get; set; }
    public virtual string City { get; set; } = string.Empty;
    public virtual string Country { get; set; } = string.Empty;

    // EF Core navigation: concrete type
    [NotMapped]
    public virtual List<csFriend> Friends { get; set; } = new();

    // Interface navigation: keep contract List<IFriend>
    // (explicit implementation so it doesn't fight with EF)
    List<IFriend> IAddress.Friends
    {
        get => Friends.Cast<IFriend>().ToList();
        set => Friends = (value ?? new List<IFriend>()).OfType<csFriend>().ToList();
    }

    #region constructors
    public Address() { }

    public Address(Address org)
    {
        Seeded = org.Seeded;

        AddressId = org.AddressId;
        StreetAddress = org.StreetAddress;
        ZipCode = org.ZipCode;
        City = org.City;
        Country = org.Country;

        Friends = new List<csFriend>();
    }
    #endregion

    #region implementing IEquatable
    public bool Equals(Address? other) =>
        (other != null) &&
        ((StreetAddress, ZipCode, City, Country) == (other.StreetAddress, other.ZipCode, other.City, other.Country));

    public override bool Equals(object? obj) => Equals(obj as Address);
    public override int GetHashCode() => (StreetAddress, ZipCode, City, Country).GetHashCode();
    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual Address Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;
        AddressId = Guid.NewGuid();

        Country = seedGenerator.Country;
        StreetAddress = seedGenerator.StreetAddress(Country);
        ZipCode = seedGenerator.ZipCode;
        City = seedGenerator.City(Country);

        Friends ??= new List<csFriend>();

        return this;
    }
    #endregion
}

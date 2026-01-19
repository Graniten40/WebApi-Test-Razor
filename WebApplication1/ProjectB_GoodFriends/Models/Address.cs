using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;

public class Address : IAddress, ISeed<Address>, IEquatable<Address>
{
    public virtual Guid AddressId { get; set; }

    public virtual string StreetAddress { get; set; }
    public virtual int ZipCode { get; set; }
    public virtual string City { get; set; }
    public virtual string Country { get; set; }

    // Model relationships
    // One Address may have many friends (= one Address can contain several residents)
    public virtual List<IFriend> Friends { get; set; } = new List<IFriend>();

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

        // Do NOT copy relationship objects here; just ensure non-null
        Friends = new List<IFriend>();
    }

    #endregion

    #region implementing IEquatable

    public bool Equals(Address other) =>
        (other != null) &&
        ((StreetAddress, ZipCode, City, Country) == (other.StreetAddress, other.ZipCode, other.City, other.Country));

    public override bool Equals(object obj) => Equals(obj as Address);

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

        // Ensure non-null relationship list even after seed
        Friends ??= new List<IFriend>();

        return this;
    }

    #endregion
}

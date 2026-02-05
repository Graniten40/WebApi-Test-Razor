using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;
using Models.DTO;

namespace DbModels;

[Table("Addresses", Schema = "supusr")]
[Index(nameof(StreetAddress), nameof(ZipCode), nameof(City), nameof(Country), IsUnique = true)]
public sealed class AddressDbM : Address, ISeed<AddressDbM>, IEquatable<AddressDbM>
{
    [Key]
    public override Guid AddressId { get; set; }

    [Required]
    public override string StreetAddress { get; set; } = string.Empty;

    [Required]
    public override int ZipCode { get; set; }

    [Required]
    public override string City { get; set; } = string.Empty;

    [Required]
    public override string Country { get; set; } = string.Empty;

    #region EF navigation (DbModel-level)
    // EF Core navigation for the relationship AddressDbM <-> FriendDbM
    // Do NOT override Address.Friends anymore (it is List<csFriend> in base class).
    [JsonIgnore]
    public List<FriendDbM> FriendsDbM { get; set; } = new();
    #endregion

    #region IEquatable
    public bool Equals(AddressDbM? other) =>
        (other != null) &&
        ((StreetAddress, ZipCode, City, Country) ==
         (other.StreetAddress, other.ZipCode, other.City, other.Country));

    public override bool Equals(object? obj) => Equals(obj as AddressDbM);
    public override int GetHashCode() => (StreetAddress, ZipCode, City, Country).GetHashCode();
    #endregion

    #region seed
    public override AddressDbM Seed(SeedGenerator seedGenerator)
    {
        base.Seed(seedGenerator);
        return this;
    }
    #endregion

    #region Update from DTO
    public AddressDbM? UpdateFromDTO(AddressCuDto org)
    {
        if (org == null) return null;

        StreetAddress = org.StreetAddress;
        ZipCode = org.ZipCode;
        City = org.City;
        Country = org.Country;

        return this;
    }
    #endregion

    #region constructors
    public AddressDbM() { }

    public AddressDbM(AddressCuDto org)
    {
        AddressId = Guid.NewGuid();
        UpdateFromDTO(org);
    }
    #endregion
}

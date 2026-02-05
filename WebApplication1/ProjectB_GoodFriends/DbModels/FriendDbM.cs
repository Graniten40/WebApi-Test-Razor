using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;
using Models.Interfaces;
using Models.DTO;

namespace DbModels;

[Table("Friends", Schema = "supusr")]
[Index(nameof(FirstName), nameof(LastName))]
[Index(nameof(LastName), nameof(FirstName))]
public sealed class FriendDbM : csFriend, ISeed<FriendDbM>
{
    [Key]
    public override Guid FriendId { get; set; }

    [Required]
    public override string FirstName { get; set; } = string.Empty;

    [Required]
    public override string LastName { get; set; } = string.Empty;

    [Required]
    public override string Email { get; set; } = string.Empty;

    // FK ska vara optional om Friend kan sakna adress
    [JsonIgnore]
    public override Guid? AddressId { get; set; }

    #region Navigation properties (EF)

    [JsonIgnore]
    [ForeignKey(nameof(AddressId))]
    public AddressDbM? AddressDbM { get; set; }

    // Basmodellen har Address (domain), vi mappar den till AddressDbM här
    [NotMapped]
    public override Address? Address
    {
        get => AddressDbM;
        set
        {
            AddressDbM = value as AddressDbM;
            AddressId = AddressDbM?.AddressId;
        }
    }

    [JsonIgnore]
    public List<PetDbM> PetsDbM { get; set; } = new();

    [NotMapped]
    public override List<IPet> Pets
    {
        get => PetsDbM.Cast<IPet>().ToList();
        set => throw new NotImplementedException("Set PetsDbM directly in DbModel.");
    }

    [JsonIgnore]
    public List<QuoteDbM> QuotesDbM { get; set; } = new();

    [NotMapped]
    public override List<IQuote> Quotes
    {
        get => QuotesDbM.Cast<IQuote>().ToList();
        set => throw new NotImplementedException("Set QuotesDbM directly in DbModel.");
    }

    #endregion

    #region randomly seed this instance

    public override FriendDbM Seed(SeedGenerator sgen)
    {
        if (sgen is null) throw new ArgumentNullException(nameof(sgen));

        // Seed endast scalar fields via base
        base.Seed(sgen);

        // Relationer sätts i AdminDbRepos (AddressDbM/PetsDbM/QuotesDbM)
        // Viktigt: lämna AddressId/AddressDbM orörda här, annars får du FK-trassel.
        // Se till att navigation-listor inte är null:
        PetsDbM ??= new List<PetDbM>();
        QuotesDbM ??= new List<QuoteDbM>();

        return this;
    }

    #endregion

    #region Update from DTO

    public FriendDbM UpdateFromDTO(FriendCuDto org)
    {
        if (org is null) throw new ArgumentNullException(nameof(org));

        FirstName = org.FirstName ?? string.Empty;
        LastName = org.LastName ?? string.Empty;
        Birthday = org.Birthday;
        Email = org.Email ?? string.Empty;

        return this;
    }

    #endregion

    #region constructors

    public FriendDbM() { }

    public FriendDbM(FriendCuDto org)
    {
        FriendId = Guid.NewGuid();
        UpdateFromDTO(org);
    }

    #endregion
}

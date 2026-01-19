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

    [JsonIgnore]
    public Guid? AddressId { get; set; }

    #region Navigation properties (EF)

    [JsonIgnore]
    [ForeignKey(nameof(AddressId))]
    public AddressDbM? AddressDbM { get; set; }

    [NotMapped]
    public override IAddress? Address
    {
        get => AddressDbM;
        set => throw new NotImplementedException("Set AddressDbM directly in DbModel.");
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

        // Viktigt: csFriend.Seed() får inte sätta Address/Pets/Quotes
        // Den ska bara seeda scalars (FirstName/LastName/Email/Birthday)
        base.Seed(sgen);

        // Seed EF navigation: AddressDbM
        AddressDbM ??= new AddressDbM();
        AddressDbM.Seed(sgen);

        // Seed EF navigation: PetsDbM
        PetsDbM ??= new List<PetDbM>();
        PetsDbM.Clear();
        if (sgen.Bool)
        {
            PetsDbM.Add(new PetDbM().Seed(sgen));
        }

        // Seed EF navigation: QuotesDbM
        QuotesDbM ??= new List<QuoteDbM>();
        QuotesDbM.Clear();
        if (sgen.Bool)
        {
            QuotesDbM.Add(new QuoteDbM().Seed(sgen));
        }

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

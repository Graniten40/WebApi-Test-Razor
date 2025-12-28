using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;
using Models.Interfaces;    
using Models.DTO;

namespace DbModels;
[Table("Pets", Schema = "supusr")]
sealed public class PetDbM : Pet, ISeed<PetDbM>
{
    [Key]    
    public override Guid PetId { get; set; }

    [JsonIgnore]
    public Guid FriendId { get; set; }  //Enforces Cascade Delete

    [Required]
    public override string Name { get; set; }

    #region adding more readability to an enum type in the database
    public string strKind
    {
        get => Kind.ToString();
        set { }  //set is needed by EFC to include in the database, so I make it to do nothing
    }
    public string strMood
    {
        get => Mood.ToString();
        set { } //set is needed by EFC to include in the database, so I make it to do nothing
    }
    #endregion
    
    #region implementing entity Navigation properties when model is using interfaces in the relationships between models
    [ForeignKey("FriendId")]     
    [JsonIgnore]
    public  FriendDbM FriendDbM { get; set; } = null;         
    [NotMapped]
    public override IFriend Friend { get => FriendDbM; set => new NotImplementedException(); }        
    #endregion

    #region randomly seed this instance
    public override PetDbM Seed(SeedGenerator sgen)
    {
        base.Seed(sgen);
        return this;
    }
    #endregion

    #region Update from DTO
    public PetDbM UpdateFromDTO(PetCuDto org)
    {
        if (org == null) return null;

        Kind = org.Kind;
        Mood = org.Mood;
        Name = org.Name;

        return this;
    }
    #endregion

    #region constructors
    public PetDbM() { }
    public PetDbM(PetCuDto org)
    {
        PetId = Guid.NewGuid();
        UpdateFromDTO(org);
    }
    #endregion
}
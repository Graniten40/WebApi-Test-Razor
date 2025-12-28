using Seido.Utilities.SeedGenerator;
using Models.Interfaces;

namespace Models;
public class Pet : IPet, ISeed<Pet>
{
    public virtual Guid PetId { get; set; }

    public virtual AnimalKind Kind { get; set; }
    public virtual AnimalMood Mood { get; set; }

    public virtual string Name { get; set; }

    // Model relationships
    // One Pet may have an owner Friend
    public virtual IFriend Friend { get; set; }


    #region constructors
    public Pet() { }
    public Pet(Pet org)
    {
        this.Seeded = org.Seeded;

        this.PetId = org.PetId;
        this.Kind = org.Kind;
        this.Name = org.Name;
    }
    #endregion

    #region randomly seed this instance
    public bool Seeded { get; set; } = false;

    public virtual Pet Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;

        PetId = Guid.NewGuid();
        Name = seedGenerator.PetName;
        Kind = seedGenerator.FromEnum<AnimalKind>();
        Mood = seedGenerator.FromEnum<AnimalMood>();

        return this;
    }
    #endregion
}



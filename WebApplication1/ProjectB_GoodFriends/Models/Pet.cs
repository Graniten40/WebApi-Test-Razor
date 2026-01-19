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
    public virtual IFriend? Friend { get; set; } = null;

    #region constructors

    public Pet() { }

    public Pet(Pet org)
    {
        Seeded = org.Seeded;

        PetId = org.PetId;
        Kind = org.Kind;
        Mood = org.Mood;
        Name = org.Name;

        // Relationship reference is typically not copied in a shallow clone
        Friend = null;
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

        // Ensure relationship starts as null (set elsewhere when linking to friend)
        Friend = null;

        return this;
    }

    #endregion
}

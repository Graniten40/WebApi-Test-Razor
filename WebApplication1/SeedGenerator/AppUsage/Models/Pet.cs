using Seido.Utilities.SeedGenerator;

namespace Models
{
    public class Pet : ISeed<Pet>, IEquatable<Pet>
    {
        public string PetName { get; set; }
        public override string ToString() => $"{PetName}";

        #region ISeed implementation to use csSeeGenerator to create random lists
        public bool Seeded { get; set; } = false;

        public Pet Seed(SeedGenerator rnd)
        {
            PetName = rnd.PetName;
            return this;
        }
        #endregion

        #region implementing IEquatable to use SeedGenerator Unique lists
        public bool Equals(Pet other) => (other != null) ? (PetName) == (other.PetName) : false;

        public override bool Equals(object obj) => Equals(obj as Pet);
        public override int GetHashCode() => (PetName).GetHashCode();
        #endregion

    }
}
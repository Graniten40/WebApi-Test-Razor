using Seido.Utilities.SeedGenerator;

namespace Models
{
    public class Person : ISeed<Person>
    {
        public string FullName { get; set; }
        public DateTime Birthday { get; set; }
        public override string ToString() => $"{FullName} is born on {Birthday:d}";

        #region ISeed implementation to use csSeeGenerator to create random lists
        public bool Seeded { get; set; } = false;

        public Person Seed(SeedGenerator rnd)
        {
            FullName = rnd.FullName;
            Birthday = rnd.DateAndTime(1970, 2010);
            return this;
        }
        #endregion
}
}
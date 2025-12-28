using System.Text.RegularExpressions;
using Models.Interfaces;


namespace Models.DTO;

//DTO is a DataTransferObject, can be instanstiated by the controller logic
//and represents a, fully instantiable, subset of the Database models
//for a specific purpose.

//These DTO are simplistic and used to Update and Create objects
public class MusicGroupCUdto
{
    public Guid? MusicGroupId { get; set; }
    public bool Seeded { get; set; } = true;

    public string Name { get; set; }
    public int EstablishedYear { get; set; }

    public MusicGenre Genre { get; set; }

    public List<Guid> AlbumsId { get; set; } = new List<Guid>();
    public List<Guid> ArtistsId { get; set; } = new List<Guid>();

    public MusicGroupCUdto() { }
    public MusicGroupCUdto(IMusicGroup model)
    {
        this.MusicGroupId = model.MusicGroupId;

        this.Name = model.Name;
        this.EstablishedYear = model.EstablishedYear;
        this.Genre = model.Genre;

        this.AlbumsId = model.Albums.Select(a => a.AlbumId).ToList();
        this.ArtistsId = model.Artists.Select(a => a.ArtistId).ToList();
    }

    public void EnsureValidity()
    {
        // RegEx check to ensure filter only contains a-z, 0-9, and spaces
        if (!string.IsNullOrEmpty(Name) && !Regex.IsMatch(Name, @"^[a-zA-Z0-9\s]*$"))
        {
            throw new ArgumentException("Name can only contain letters (a-z), numbers (0-9), and spaces.");
        }
        if (EstablishedYear <= 0) throw new ArgumentException("EstablishedYear has to be larger than zero");
        if (!Enum.IsDefined(typeof(MusicGenre), Genre)) throw new ArgumentException("Genre has to be set to a valid value");
    }
}

public class AlbumCUdto
{
    public Guid? AlbumId { get; set; }
    public bool Seeded { get; set; } = true;

    public string Name { get; set; }
    public int ReleaseYear { get; set; }
    public long CopiesSold { get; set; }

    //Navigation properties that EFC will use to build relations
    public Guid MusicGroupId { get; set; }


    public AlbumCUdto() { }
    public AlbumCUdto(IAlbum model)
    {
        this.AlbumId = model.AlbumId;

        this.Name = model.Name;
        this.ReleaseYear = model.ReleaseYear;
        this.CopiesSold = model.CopiesSold;

        this.MusicGroupId = model.MusicGroup.MusicGroupId;
    }

    public void EnsureValidity()
    {
        // RegEx check to ensure filter only contains a-z, 0-9, and spaces
        if (!string.IsNullOrEmpty(Name) && !Regex.IsMatch(Name, @"^[a-zA-Z0-9\s]*$"))
        {
            throw new ArgumentException("Name can only contain letters (a-z), numbers (0-9), and spaces.");
        }
        if (ReleaseYear <= 0) throw new ArgumentException("ReleaseYear has to be larger than zero");
        if (CopiesSold <= 0) throw new ArgumentException("CopiesSold has to be larger than zero");
    }
}

public class ArtistCUdto
{
    public Guid? ArtistId { get; set; }
    public bool Seeded { get; set; } = true;

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime? BirthDay { get; set; }

    //Navigation properties that EFC will use to build relations
    public List<Guid> MusicGroupsId { get; set; } = null;


    public ArtistCUdto() { }
    public ArtistCUdto(IArtist model)
    {
        this.ArtistId = model.ArtistId;

        this.FirstName = model.FirstName;
        this.LastName = model.LastName;
        this.BirthDay = model.BirthDay;

        this.MusicGroupsId = model.MusicGroups?.Select(a => a.MusicGroupId).ToList();
    }

    public void EnsureValidity()
    {
        // RegEx check to ensure filter only contains a-z, 0-9, and spaces
        if (!string.IsNullOrEmpty(FirstName) && !Regex.IsMatch(FirstName, @"^[a-zA-Z0-9\s]*$"))
        {
            throw new ArgumentException("FirstName can only contain letters (a-z), numbers (0-9), and spaces.");
        }
        if (!string.IsNullOrEmpty(LastName) && !Regex.IsMatch(LastName, @"^[a-zA-Z0-9\s]*$"))
        {
            throw new ArgumentException("LastName can only contain letters (a-z), numbers (0-9), and spaces.");
        }
        if (BirthDay.HasValue)
        {
            // Use DateTime.Parse to validate the date by converting back to string and parsing
            var dateString = BirthDay.Value.ToString("yyyy-MM-dd");
            var parsedDate = DateTime.Parse(dateString);

            // Additional checks for reasonable birthday range
            if (parsedDate != BirthDay.Value || parsedDate.Year < 0 || parsedDate > DateTime.Now)
            {
                throw new ArgumentException("BirthDay must be a valid date in the past (after year 0) or null.");
            }
        }
    }
}





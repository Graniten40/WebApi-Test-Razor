using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Seido.Utilities.SeedGenerator
{
    #region exported types
    public interface ISeed<T>
    {
        // In order to separate from real and seeded instances
        public bool Seeded { get; set; }

        // Seeded the instance
        public T Seed(SeedGenerator seedGenerator);
    }

    public class SeededLatin
    {
        public string Paragraph { get; init; }
        public List<string> Sentences { get; init; }
        public List<string> Words { get; init; }
    }

    public class SeededQuote
    {
        public string Quote { get; init; }
        public string Author { get; init; }
    }
    #endregion

    public class SeedGenerator : Random
    {
        readonly SeedJsonContent _seeds;

        #region Names
        public string PetName => _seeds.Names.PetNames[this.Next(0, _seeds.Names.PetNames.Count)];
        public string FirstName => _seeds.Names.FirstNames[this.Next(0, _seeds.Names.FirstNames.Count)];
        public string LastName => _seeds.Names.LastNames[this.Next(0, _seeds.Names.LastNames.Count)];
        public string FullName => $"{FirstName} {LastName}";
        #endregion

        #region Addresses
        public string Country => _seeds.Addresses[this.Next(0, _seeds.Addresses.Count)].Country;

        public string City(string Country = null)
        {
            if (Country != null)
            {
                var adr = _seeds.Addresses.FirstOrDefault(c => c.Country.ToLower() == Country.Trim().ToLower());
                if (adr == null)
                    throw new ArgumentException("Country not found");

                return adr.Cities[this.Next(0, adr.Cities.Count)];
            }

            var tmp = _seeds.Addresses[this.Next(0, _seeds.Addresses.Count)];
            return tmp.Cities[this.Next(0, tmp.Cities.Count)];
        }

        public string StreetAddress(string Country = null)
        {
            if (Country != null)
            {
                var adr = _seeds.Addresses.FirstOrDefault(c => c.Country.ToLower() == Country.Trim().ToLower());
                if (adr == null)
                    throw new ArgumentException("Country not found");

                return $"{adr.Streets[this.Next(0, adr.Streets.Count)]} {this.Next(1, 100)}";
            }

            var tmp = _seeds.Addresses[this.Next(0, _seeds.Addresses.Count)];
            return $"{tmp.Streets[this.Next(0, tmp.Streets.Count)]} {this.Next(1, 100)}";
        }

        public int ZipCode => this.Next(10101, 100000);
        #endregion

        #region Emails and phones
        public string Email(string fname = null, string lname = null)
        {
            fname ??= FirstName;
            lname ??= LastName;

            return $"{fname}.{lname}@{_seeds.Domains.Domains[this.Next(0, _seeds.Domains.Domains.Count)]}";
        }

        public string PhoneNr => $"{this.Next(700, 800)} {this.Next(100, 1000)} {this.Next(100, 1000)}";
        #endregion

        #region Quotes
        public List<SeededQuote> AllQuotes => _seeds.Quotes
            .Select(q => new SeededQuote { Quote = q.Quote, Author = q.Author })
            .ToList();

        public List<SeededQuote> Quotes(int tryNrOfItems)
        {
            return UniqueIndexPickedFromList(tryNrOfItems, AllQuotes);
        }

        public SeededQuote Quote => Quotes(1).FirstOrDefault();
        #endregion

        #region Latin
        public List<SeededLatin> AllLatin => _seeds.Latin
            .Select(l => new SeededLatin { Paragraph = l.Paragraph, Sentences = l.Sentences, Words = l.Words })
            .ToList();

        public List<SeededLatin> LatinParagraphs(int tryNrOfItems)
        {
            return UniqueIndexPickedFromList(tryNrOfItems, AllLatin);
        }

        public List<string> LatinSentences(int tryNrOfItems)
        {
            var sRet = new List<string>();
            for (int i = 0; i < tryNrOfItems; i++)
            {
                var pIdx = this.Next(0, AllLatin.Count);
                var sIdx = this.Next(0, AllLatin[pIdx].Sentences.Count);

                sRet.Add(AllLatin[pIdx].Sentences[sIdx]);
            }
            return sRet;
        }

        public List<string> LatinWords(int tryNrOfItems)
        {
            var sRet = new List<string>();
            for (int i = 0; i < tryNrOfItems; i++)
            {
                var pIdx = this.Next(0, AllLatin.Count);
                var wIdx = this.Next(0, AllLatin[pIdx].Words.Count);

                sRet.Add(AllLatin[pIdx].Words[wIdx]);
            }
            return sRet;
        }

        public string LatinParagraph => LatinParagraphs(1).FirstOrDefault()?.Paragraph;
        public string LatinSentence => LatinSentences(1).FirstOrDefault();
        #endregion

        #region Music
        public string MusicGroupName => "The " + _seeds.Music.GroupNames[this.Next(0, _seeds.Music.GroupNames.Count)]
            + " " + _seeds.Music.GroupNames[this.Next(0, _seeds.Music.GroupNames.Count)];

        public string MusicAlbumName => _seeds.Music.AlbumPrefix[this.Next(0, _seeds.Music.AlbumPrefix.Count)]
            + " " + _seeds.Music.AlbumNames[this.Next(0, _seeds.Music.AlbumNames.Count)]
            + " " + _seeds.Music.AlbumNames[this.Next(0, _seeds.Music.AlbumNames.Count)]
            + " " + _seeds.Music.AlbumSuffix[this.Next(0, _seeds.Music.AlbumSuffix.Count)];
        #endregion

        #region validation
        private void ValidateSeeds()
        {
            if (_seeds is null)
                throw new InvalidOperationException("SeedGenerator: _seeds is null. Seed file could not be loaded/deserialized.");

            if (_seeds.Names is null)
                throw new InvalidOperationException("SeedGenerator: Names section is missing (null).");

            if (_seeds.Names.FirstNames is null || _seeds.Names.FirstNames.Count == 0)
                throw new InvalidOperationException("SeedGenerator: Names.FirstNames is null/empty.");

            if (_seeds.Names.LastNames is null || _seeds.Names.LastNames.Count == 0)
                throw new InvalidOperationException("SeedGenerator: Names.LastNames is null/empty.");

            if (_seeds.Domains is null || _seeds.Domains.Domains is null || _seeds.Domains.Domains.Count == 0)
                throw new InvalidOperationException("SeedGenerator: Domains.Domains is null/empty.");

            if (_seeds.Addresses is null || _seeds.Addresses.Count == 0)
                throw new InvalidOperationException("SeedGenerator: Addresses is null/empty.");
        }
        #endregion


        #region DateTime, bool and decimal
        public DateTime DateAndTime(int? fromYear = null, int? toYear = null)
        {
            bool dateOK = false;
            DateTime _date = default;
            while (!dateOK)
            {
                fromYear ??= DateTime.Today.Year;
                toYear ??= DateTime.Today.Year + 1;

                try
                {
                    int year = this.Next(Math.Min(fromYear.Value, toYear.Value),
                        Math.Max(fromYear.Value, toYear.Value));
                    int month = this.Next(1, 13);
                    int day = this.Next(1, 32);

                    _date = new DateTime(year, month, day);
                    dateOK = true;
                }
                catch
                {
                    dateOK = false;
                }
            }

            return DateTime.SpecifyKind(_date, DateTimeKind.Utc);
        }

        public bool Bool => (this.Next(0, 10) < 5);

        public decimal NextDecimal(int _from, int _to) => this.Next(_from * 1000, _to * 1000) / 1000M;
        #endregion

        #region From own String, Enum and List<TItem>
        public string FromString(string _inputString, string _splitDelimiter = ", ")
        {
            var _sarray = _inputString.Split(_splitDelimiter);
            return _sarray[this.Next(0, _sarray.Length)];
        }

        public TEnum FromEnum<TEnum>() where TEnum : struct
        {
            if (typeof(TEnum).IsEnum)
            {
                var _names = typeof(TEnum).GetEnumNames();
                var _name = _names[this.Next(0, _names.Length)];
                return Enum.Parse<TEnum>(_name);
            }
            throw new ArgumentException("Not an enum type");
        }

        public TItem FromList<TItem>(List<TItem> items)
        {
            return items[this.Next(0, items.Count)];
        }
        #endregion

        #region Generate seeded List of TItem
        public List<TItem> ItemsToList<TItem>(int NrOfItems)
            where TItem : ISeed<TItem>, new()
        {
            var _list = new List<TItem>();
            for (int c = 0; c < NrOfItems; c++)
            {
                _list.Add(new TItem() { Seeded = true }.Seed(this));
            }
            return _list;
        }

        public List<TItem> UniqueItemsToList<TItem>(int tryNrOfItems, List<TItem> appendToUnique = null)
            where TItem : ISeed<TItem>, IEquatable<TItem>, new()
        {
            HashSet<TItem> _set = (appendToUnique == null) ? new HashSet<TItem>() : new HashSet<TItem>(appendToUnique);

            while (_set.Count < tryNrOfItems)
            {
                var _item = new TItem() { Seeded = true }.Seed(this);

                int _preCount = _set.Count;
                int tries = 0;
                do
                {
                    _set.Add(_item);

                    if (_set.Count == _preCount)
                    {
                        _item = new TItem() { Seeded = true }.Seed(this);
                        ++tries;

                        if (tries > 5)
                            return _set.ToList();
                    }

                } while (_set.Count <= _preCount);
            }

            return _set.ToList();
        }

        public List<TItem> UniqueItemsPickedFromList<TItem>(int tryNrOfItems, List<TItem> list)
            where TItem : IEquatable<TItem>
        {
            HashSet<TItem> _set = new HashSet<TItem>();

            while (_set.Count < tryNrOfItems)
            {
                var _item = list[this.Next(0, list.Count)];

                int _preCount = _set.Count;
                int tries = 0;
                do
                {
                    _set.Add(_item);

                    if (_set.Count == _preCount)
                    {
                        _item = list[this.Next(0, list.Count)];
                        ++tries;

                        if (tries > 5)
                            return _set.ToList();
                    }

                } while (_set.Count <= _preCount);
            }

            return _set.ToList();
        }

        public List<TItem> UniqueIndexPickedFromList<TItem>(int tryNrOfItems, List<TItem> list)
            where TItem : new()
        {
            HashSet<int> _set = new HashSet<int>();

            while (_set.Count < tryNrOfItems)
            {
                var _idx = this.Next(0, list.Count);

                int _preCount = _set.Count;
                int tries = 0;
                do
                {
                    _set.Add(_idx);

                    if (_set.Count == _preCount)
                    {
                        _idx = this.Next(0, list.Count);
                        ++tries;

                        if (tries > 5)
                            break;
                    }

                } while (_set.Count <= _preCount);
            }

            var retList = new List<TItem>();
            foreach (var item in _set)
            {
                retList.Add(list[item]);
            }
            return retList;
        }
        #endregion

        #region initialize master content
        // (CreateMasterSeedFile unchanged)
        SeedJsonContent CreateMasterSeedFile()
        {
            // Your existing CreateMasterSeedFile() content goes here unchanged
            // (omitted in this snippet for brevity)
            throw new NotImplementedException("Paste your existing CreateMasterSeedFile() implementation here.");
        }
        #endregion

        #region create master json file
        public string WriteMasterStream()
        {
            return CreateMasterSeedFile().WriteFile("master-seeds.json");
        }
        #endregion

        #region contructors
        public SeedGenerator()
        {
            _seeds = CreateMasterSeedFile();
            ValidateSeeds();
        }

        public SeedGenerator(string SeedPathName)
        {
            if (!SeedJsonContent.FileExists(SeedPathName))
                throw new FileNotFoundException(SeedPathName);

            _seeds = SeedJsonContent.ReadFile(SeedPathName);
            ValidateSeeds();
        }
        #endregion


        // #region validation
        // private void ValidateSeeds()
        // {
        //     if (_seeds is null) throw new InvalidOperationException("_seeds is null.");

        //     if (_seeds.Names is null) throw new InvalidOperationException("_seeds.Names is null.");
        //     if (_seeds.Names.FirstNames is null || _seeds.Names.FirstNames.Count == 0) throw new InvalidOperationException("No FirstNames loaded.");
        //     if (_seeds.Names.LastNames is null || _seeds.Names.LastNames.Count == 0) throw new InvalidOperationException("No LastNames loaded.");
        //     if (_seeds.Names.PetNames is null || _seeds.Names.PetNames.Count == 0) throw new InvalidOperationException("No PetNames loaded.");

        //     if (_seeds.Domains is null) throw new InvalidOperationException("_seeds.Domains is null.");
        //     if (_seeds.Domains.Domains is null || _seeds.Domains.Domains.Count == 0) throw new InvalidOperationException("No Domains loaded.");

        //     if (_seeds.Addresses is null || _seeds.Addresses.Count == 0) throw new InvalidOperationException("No Addresses loaded.");

        //     if (_seeds.Music is null) throw new InvalidOperationException("_seeds.Music is null.");
        //     if (_seeds.Music.GroupNames is null || _seeds.Music.GroupNames.Count == 0) throw new InvalidOperationException("No Music GroupNames loaded.");
        //     if (_seeds.Music.AlbumNames is null || _seeds.Music.AlbumNames.Count == 0) throw new InvalidOperationException("No Music AlbumNames loaded.");
        //     if (_seeds.Music.AlbumPrefix is null || _seeds.Music.AlbumPrefix.Count == 0) throw new InvalidOperationException("No Music AlbumPrefix loaded.");
        //     if (_seeds.Music.AlbumSuffix is null || _seeds.Music.AlbumSuffix.Count == 0) throw new InvalidOperationException("No Music AlbumSuffix loaded.");

        //     if (_seeds.Latin is null || _seeds.Latin.Count == 0) throw new InvalidOperationException("No Latin loaded.");
        //     if (_seeds.Quotes is null || _seeds.Quotes.Count == 0) throw new InvalidOperationException("No Quotes loaded.");
        // }
        // #endregion

        #region internal classes
        class SeedLatin
        {
            string _jsonParagraph;

            public string jsonParagraph
            {
                get => _jsonParagraph;
                set
                {
                    _jsonParagraph = value ?? string.Empty;

                    _sentences = string.IsNullOrWhiteSpace(_jsonParagraph)
                        ? new List<string>()
                        : new List<string>(_jsonParagraph.Split(". "))
                            .Select(s =>
                            {
                                var _sentence = s.Trim(new char[] { ' ', ',', '.' });
                                return string.IsNullOrWhiteSpace(_sentence) ? null : _sentence + '.';
                            })
                            .Where(s => s != null)
                            .ToList();

                    _words = string.IsNullOrWhiteSpace(_jsonParagraph)
                        ? new List<string>()
                        : new List<string>(_jsonParagraph.Split(" "))
                            .Select(w => w.Trim(new char[] { ' ', ',', '.' }))
                            .Where(w => !string.IsNullOrWhiteSpace(w))
                            .ToList();
                }
            }

            [JsonIgnore]
            public string Paragraph => _jsonParagraph ?? string.Empty;

            List<string> _sentences = new List<string>();
            [JsonIgnore]
            public List<string> Sentences => _sentences ??= new List<string>();

            List<string> _words = new List<string>();
            [JsonIgnore]
            public List<string> Words => _words ??= new List<string>();
        }

        class SeedQuote
        {
            string _jsonQuote;
            public string jsonQuote { get => _jsonQuote; set => _jsonQuote = value; }

            string _jsonAuthor;
            public string jsonAuthor { get => _jsonAuthor; set => _jsonAuthor = value; }

            [JsonIgnore]
            public string Quote => _jsonQuote ?? string.Empty;
            [JsonIgnore]
            public string Author => _jsonAuthor ?? string.Empty;
        }

        class SeedAddress
        {
            string _jsonCountry;
            public string jsonCountry { get => _jsonCountry; set { _jsonCountry = value; } }

            [JsonIgnore]
            public string Country => _jsonCountry ?? string.Empty;

            string _jsonStreets;
            public string jsonStreets
            {
                get => _jsonStreets;
                set
                {
                    _jsonStreets = value ?? string.Empty;
                    _streets = string.IsNullOrWhiteSpace(_jsonStreets)
                        ? new List<string>()
                        : _jsonStreets.Split(", ").ToList();
                }
            }

            List<string> _streets = new List<string>();
            [JsonIgnore]
            public List<string> Streets => _streets ??= new List<string>();

            string _jsonCities;
            public string jsonCities
            {
                get => _jsonCities;
                set
                {
                    _jsonCities = value ?? string.Empty;
                    _cities = string.IsNullOrWhiteSpace(_jsonCities)
                        ? new List<string>()
                        : _jsonCities.Split(", ").ToList();
                }
            }

            List<string> _cities = new List<string>();
            [JsonIgnore]
            public List<string> Cities => _cities ??= new List<string>();
        }

        class SeedNames
        {
            string _jsonFirstNames;
            public string jsonFirstNames
            {
                get => _jsonFirstNames;
                set
                {
                    _jsonFirstNames = value ?? string.Empty;
                    _firstNames = string.IsNullOrWhiteSpace(_jsonFirstNames)
                        ? new List<string>()
                        : _jsonFirstNames.Split(", ").ToList();
                }
            }

            string _jsonLastNames;
            public string jsonLastNames
            {
                get => _jsonLastNames;
                set
                {
                    _jsonLastNames = value ?? string.Empty;
                    _lastNames = string.IsNullOrWhiteSpace(_jsonLastNames)
                        ? new List<string>()
                        : _jsonLastNames.Split(", ").ToList();
                }
            }

            string _jsonPetNames;
            public string jsonPetNames
            {
                get => _jsonPetNames;
                set
                {
                    _jsonPetNames = value ?? string.Empty;
                    _petNames = string.IsNullOrWhiteSpace(_jsonPetNames)
                        ? new List<string>()
                        : _jsonPetNames.Split(", ").ToList();
                }
            }

            List<string> _firstNames = new List<string>();
            [JsonIgnore]
            public List<string> FirstNames => _firstNames ??= new List<string>();

            List<string> _lastNames = new List<string>();
            [JsonIgnore]
            public List<string> LastNames => _lastNames ??= new List<string>();

            List<string> _petNames = new List<string>();
            [JsonIgnore]
            public List<string> PetNames => _petNames ??= new List<string>();
        }

        class SeedDomains
        {
            string _jsonDomainNames;
            public string jsonDomainNames
            {
                get => _jsonDomainNames;
                set
                {
                    _jsonDomainNames = value ?? string.Empty;
                    _domainNames = string.IsNullOrWhiteSpace(_jsonDomainNames)
                        ? new List<string>()
                        : _jsonDomainNames.Split(", ").ToList();
                }
            }

            List<string> _domainNames = new List<string>();
            [JsonIgnore]
            public List<string> Domains => _domainNames ??= new List<string>();
        }

        class SeedMusic
        {
            string _jsonGroupNames;
            public string jsonGroupNames
            {
                get => _jsonGroupNames;
                set
                {
                    _jsonGroupNames = value ?? string.Empty;
                    _groupNames = string.IsNullOrWhiteSpace(_jsonGroupNames)
                        ? new List<string>()
                        : _jsonGroupNames.Split(", ").ToList();
                }
            }

            string _jsonAlbumNames;
            public string jsonAlbumNames
            {
                get => _jsonAlbumNames;
                set
                {
                    _jsonAlbumNames = value ?? string.Empty;
                    _albumNames = string.IsNullOrWhiteSpace(_jsonAlbumNames)
                        ? new List<string>()
                        : _jsonAlbumNames.Split(", ").ToList();
                }
            }

            string _jsonAlbumPrefix;
            public string jsonAlbumPrefix
            {
                get => _jsonAlbumPrefix;
                set
                {
                    _jsonAlbumPrefix = value ?? string.Empty;
                    _albumPrefix = string.IsNullOrWhiteSpace(_jsonAlbumPrefix)
                        ? new List<string>()
                        : _jsonAlbumPrefix.Split(", ").ToList();
                }
            }

            string _jsonAlbumSuffix;
            public string jsonAlbumSuffix
            {
                get => _jsonAlbumSuffix;
                set
                {
                    _jsonAlbumSuffix = value ?? string.Empty;
                    _albumSuffix = string.IsNullOrWhiteSpace(_jsonAlbumSuffix)
                        ? new List<string>()
                        : _jsonAlbumSuffix.Split(", ").ToList();
                }
            }

            List<string> _groupNames = new List<string>();
            [JsonIgnore]
            public List<string> GroupNames => _groupNames ??= new List<string>();

            List<string> _albumNames = new List<string>();
            [JsonIgnore]
            public List<string> AlbumNames => _albumNames ??= new List<string>();

            List<string> _albumPrefix = new List<string>();
            [JsonIgnore]
            public List<string> AlbumPrefix => _albumPrefix ??= new List<string>();

            List<string> _albumSuffix = new List<string>();
            [JsonIgnore]
            public List<string> AlbumSuffix => _albumSuffix ??= new List<string>();
        }

        class SeedJsonContent
        {
            public List<SeedQuote> Quotes { get; set; } = new List<SeedQuote>();
            public List<SeedLatin> Latin { get; set; } = new List<SeedLatin>();
            public List<SeedAddress> Addresses { get; set; } = new List<SeedAddress>();
            public SeedNames Names { get; set; } = new SeedNames();
            public SeedDomains Domains { get; set; } = new SeedDomains();
            public SeedMusic Music { get; set; } = new SeedMusic();

            public string WriteFile(string FileName) => WriteFile(this, FileName);

            public static string WriteFile(SeedJsonContent Seeds, string FileName)
            {
                var fn = fname(FileName);
                using (Stream s = File.Create(fn))
                using (TextWriter writer = new StreamWriter(s))
                {
                    writer.Write(JsonConvert.SerializeObject(Seeds, Formatting.Indented));
                }
                return fn;
            }

            public static SeedJsonContent ReadFile(string pathName)
            {
                if (string.IsNullOrWhiteSpace(pathName))
                    throw new ArgumentException("PathName is empty.", nameof(pathName));

                var fn = Path.GetFileName(pathName);
                if (fn == pathName)
                    pathName = fname(pathName);

                using Stream s = File.OpenRead(pathName);
                using TextReader reader = new StreamReader(s);

                var json = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException($"Seed file is empty: '{pathName}'.");

                var seeds = JsonConvert.DeserializeObject<SeedJsonContent>(json);
                if (seeds is null)
                    throw new InvalidOperationException($"Could not deserialize seed file: '{pathName}'.");

                // Viktigt: fånga "seeds finns men innehållet är inte användbart"
                seeds.ValidateOrThrow(pathName);

                return seeds;
            }



            public static string ResolvePath(string fileNameOrPath)
            {
                var fn = Path.GetFileName(fileNameOrPath);
                if (fn == fileNameOrPath)
                {
                    // no path in input -> use default directory
                    return fname(fileNameOrPath);
                }

                return fileNameOrPath;
            }

            static string fname(string name)
            {
                var documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                documentPath = Path.Combine(documentPath, "SeedGenerator");
                if (!Directory.Exists(documentPath)) Directory.CreateDirectory(documentPath);
                return Path.Combine(documentPath, name);
            }

            public static bool FileExists(string FileName)
            {
                var resolved = ResolvePath(FileName);
                return File.Exists(resolved);
            }

            public void ValidateOrThrow(string source)
            {
                if (Names?.FirstNames == null || Names.FirstNames.Count == 0)
                    throw new InvalidOperationException($"SeedJsonContent invalid from '{source}': Names.FirstNames is null/empty.");

                if (Names?.LastNames == null || Names.LastNames.Count == 0)
                    throw new InvalidOperationException($"SeedJsonContent invalid from '{source}': Names.LastNames is null/empty.");

                if (Domains?.Domains == null || Domains.Domains.Count == 0)
                    throw new InvalidOperationException($"SeedJsonContent invalid from '{source}': Domains.Domains is null/empty.");

                if (Addresses == null || Addresses.Count == 0)
                    throw new InvalidOperationException($"SeedJsonContent invalid from '{source}': Addresses is null/empty.");

                if (Addresses.Any(a => a.Streets == null || a.Streets.Count == 0 || a.Cities == null || a.Cities.Count == 0))
                    throw new InvalidOperationException($"SeedJsonContent invalid from '{source}': Some Addresses have null/empty Streets/Cities.");
            }

        }
        #endregion
    }
}

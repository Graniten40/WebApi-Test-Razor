using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Seido.Utilities.SeedGenerator;

using Models;

namespace AppSeeding
{
    public enum Greetings { Hello, Goodbye, GoodMorning, GoodEvening }
    
    class Program
    {
        static void Main(string[] args)
        {
            #region SeedGenerator Usage Examples
            Console.WriteLine("SeedGenerator Usage Examples");

            //Create a generator, inherited from .NET Random
            var rnd = new SeedGenerator();

            Console.WriteLine("Random Names");
            Console.WriteLine($"Firstname: {rnd.FirstName}");
            Console.WriteLine($"Lastname: {rnd.LastName}");
            Console.WriteLine($"Fullname: {rnd.FullName}");
            Console.WriteLine($"Petname: {rnd.PetName}");

            Console.WriteLine("\nRandom Address");
            var _country = rnd.Country;
            Console.WriteLine($"Streetname: {rnd.StreetAddress(_country)}");
            Console.WriteLine($"City: {rnd.City(_country)}");
            Console.WriteLine($"Zip code: {rnd.ZipCode}");
            Console.WriteLine($"Country: {_country}");

            Console.WriteLine("\nRandom Email and Phone number");
            Console.WriteLine($"Email: {rnd.Email()}");
            Console.WriteLine($"Email for specific name: {rnd.Email("John", "Smith")}");
            Console.WriteLine($"Phone number: {rnd.PhoneNr}");

            Console.WriteLine("\nRandom Quote");
            var _quote = rnd.Quote;
            Console.WriteLine($"Famous Quote: {_quote.Quote}");
            Console.WriteLine($"Author: {_quote.Author}");

            Console.WriteLine("\nBogus Random Latin");
            Console.WriteLine($"Latin paragraph:\n{rnd.LatinParagraph}");
            Console.WriteLine($"\nLatin sentence:\n{rnd.LatinSentence}");
            Console.WriteLine($"\n3 Latin sentences:\n{string.Join(" ", rnd.LatinSentences(3))}");
            Console.WriteLine($"\n10 Latin words:\n{string.Join(", ", rnd.LatinWords(10))}");
            Console.WriteLine($"\nLatin words as sentence:\n{rnd.LatinWordsAsSentence(7)}");
            Console.WriteLine($"{rnd.LatinWordsAsSentence(5, "!")}");
            Console.WriteLine($"{rnd.LatinWordsAsSentence(3, "?")}");

            Console.WriteLine("\nRandom Music group and album names");
            Console.WriteLine($"Music group name: {rnd.MusicGroupName}");
            Console.WriteLine($"Music album name: {rnd.MusicAlbumName}");
            Console.WriteLine($"Music album name: {rnd.MusicAlbumName}");
            Console.WriteLine($"Album prefix: {rnd.AlbumPrefix}");
            Console.WriteLine($"Album suffix: {rnd.AlbumSuffix}");

            Console.WriteLine("\nDateAndTime and Bool");
            Console.WriteLine($"This Year: {rnd.DateAndTime()}");
            Console.WriteLine($"Between Years: {rnd.DateAndTime(2000, 2020)}");
            Console.WriteLine($"True or False: {rnd.Bool}");

            Console.WriteLine("\nFrom String, Enum and List");

            Console.WriteLine($"From String: {rnd.FromString("Quick, brown, fox")}");
            Console.WriteLine($"From Enum {nameof(Greetings)}: {rnd.FromEnum<Greetings>()}");

            var f = "Cloudy, Stormy, Rainy, Sunny, Windy";
            List<Weather> _forecast = new List<Weather>
            {
                new Weather{ Temp = rnd.NextDecimal(100, 300), Visibility = rnd.FromString(f)},
                new Weather{ Temp = rnd.NextDecimal(100, 300), Visibility = rnd.FromString(f)},
                new Weather{ Temp = rnd.NextDecimal(100, 300), Visibility = rnd.FromString(f)}
            };
            Console.WriteLine($"From List {nameof(Weather)} : {rnd.FromList(_forecast)}");

            Console.WriteLine("\nGenerating a randomly seeded list");
            var _persons = rnd.ItemsToList<Person>(10);
            foreach (var item in _persons)
            {
                Console.WriteLine(item);
            }


            Console.WriteLine("\nGenerating a list of unique, randomly seeded, items");
            int _tryNrItems = 1000;
            var _pets = rnd.UniqueItemsToList<Pet>(_tryNrItems);
            Console.WriteLine($"Try to generate {_tryNrItems} unique {nameof(Pet)}");
            Console.WriteLine($"{_pets.Count} unique {nameof(Pet)} could be created");
            foreach (var item in _pets)
            {
                Console.WriteLine(item);
            }


            Console.WriteLine("\nPicking unique items from a List");

            var _picklist = "Morning, Evening, Morning, Afternoon, Afternoon".Split(", ");
            var _uniquePicks = rnd.UniqueItemsPickedFromList<string>(_tryNrItems, _picklist.ToList());
            Console.WriteLine($"Try to pick {_tryNrItems} unique items from {nameof(_picklist)}");
            Console.WriteLine($"{_uniquePicks.Count} unique items could be picked");
            foreach (var item in _uniquePicks)
            {
                Console.WriteLine(item);
            }


            var _AnotherPicklist = rnd.ItemsToList<Pet>(10000);
            var _AnotherUniquePicks = rnd.UniqueItemsPickedFromList<Pet>(_tryNrItems, _AnotherPicklist);
            Console.WriteLine($"\nTry to pick {_tryNrItems} unique items from {nameof(_AnotherPicklist)}");
            Console.WriteLine($"{_AnotherUniquePicks.Count} unique items could be picked");
            foreach (var item in _AnotherUniquePicks)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("\nWrite seeds to master-seeds.json file");
            var fn = rnd.WriteMasterStream();
            Console.WriteLine(fn);


            Console.WriteLine("\nRead seeds from master-seeds.json file");
            try
            {
                fn = "./master-seeds.json";
                System.Console.WriteLine(Path.GetFullPath(fn));
                var rndMySeeds = new SeedGenerator(fn);


                Console.WriteLine("Random Names using master-seeds.json file");
                Console.WriteLine(fn);     

                Console.WriteLine($"Firstname: {rndMySeeds.FirstName}");
                Console.WriteLine($"Lastname: {rndMySeeds.LastName}");
                Console.WriteLine($"Fullname: {rndMySeeds.FullName}");
                Console.WriteLine($"Petname: {rndMySeeds.PetName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not read seeds from master-seed.json file");
                Console.WriteLine($"Error {ex.GetType()} {ex.Message}");
            }

            #endregion
        }
    }
}







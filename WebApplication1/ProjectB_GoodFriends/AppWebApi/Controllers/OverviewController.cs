using DbContext;                 // MainDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWebApi.Controllers;

// Markerar klassen som en API-controller:
// - Automatisk model validation (400 på invalid model state i många fall)
// - Bättre binding/conventions
[ApiController]

// Basroute för hela controllern: alla endpoints hamnar under /api/overview/...
[Route("api/overview")]
public class OverviewController : ControllerBase
{
    // DI-injicerad DbContext för att köra EF Core-queries mot databasen
    private readonly MainDbContext _db;

    // Konstruktor: tar emot MainDbContext via dependency injection
    public OverviewController(MainDbContext db)
    {
        _db = db;
    }

    // GET /api/overview/friends-by-country
    // Returnerar en "översikt per land":
    // - landets namn (trimmat)
    // - antal friends i landet
    // - antal unika städer i landet (där City inte är null/whitespace)
    [HttpGet("friends-by-country")]
    public async Task<IActionResult> FriendsByCountry()
    {
        // Bygger en EF Core-query som körs i databasen (SQL) vid ToListAsync()
        var result = await _db.Friends
            // Filtrerar bort friends utan Address eller utan Country
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country))

            // Grupperar på Country, efter Trim().
            // "!" betyder: du lovar att AddressDbM inte är null här (du har filtrerat ovan).
            .GroupBy(f => f.AddressDbM!.Country.Trim())

            // Skapar ett anonymt objekt som blir JSON i API-svaret
            .Select(g => new
            {
                // Gruppens nyckel = landet
                Country = g.Key,

                // Antal friends i landet
                TotalFriends = g.Count(),

                // Räknar antalet unika städer i landet:
                // - plockar ut City
                // - filtrerar bort null/whitespace
                // - Trim()
                // - Distinct()
                // - Count()
                Cities = g.Select(x => x.AddressDbM!.City)
                          .Where(c => !string.IsNullOrWhiteSpace(c))
                          .Select(c => c!.Trim())
                          .Distinct()
                          .Count()
            })

            // Sorterar länder efter flest friends först
            .OrderByDescending(x => x.TotalFriends)

            // Exekverar queryn async
            .ToListAsync();

        // Returnerar 200 OK med JSON-array
        return Ok(result);
    }

    // GET /api/overview/friends-by-country-city
    // Returnerar en lista per land + stad:
    // - Country (trimmat)
    // - City (trimmat, eller "-" om tom)
    // - NrFriends (antal friends i den kombinationen)
    [HttpGet("friends-by-country-city")]
    public async Task<IActionResult> FriendsByCountryCity()
    {
        var result = await _db.Friends
            // Samma grundfilter: måste ha Address och Country
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country))

            // Grupperar på ett composite key-objekt:
            // - Country = trim
            // - City = "-" om tom, annars trim
            .GroupBy(f => new
            {
                Country = f.AddressDbM!.Country.Trim(),
                City = string.IsNullOrWhiteSpace(f.AddressDbM!.City) ? "-" : f.AddressDbM!.City.Trim()
            })

            // Projektar ut gruppresultat som JSON
            .Select(g => new
            {
                Country = g.Key.Country,
                City = g.Key.City,
                NrFriends = g.Count()
            })

            // Sortering:
            // 1) Country A-Ö
            // 2) Inom land: flest friends först
            // 3) Inom samma antal: City A-Ö
            .OrderBy(x => x.Country)
            .ThenByDescending(x => x.NrFriends)
            .ThenBy(x => x.City)
            .ToListAsync();

        return Ok(result);
    }

    // NEW: Overview Friends & Pets in the cities of a specific country
    // GET /api/overview/cities/{country}
    // Returnerar per stad i ett specifikt land:
    // - FriendsCount i staden
    // - PetsCount i staden (summerat över friends i staden)
    [HttpGet("cities/{country}")]
    public async Task<IActionResult> CitiesInCountry(string country)
    {
        // Enkel input-validering:
        // om route-parametern är tom => 400 BadRequest
        if (string.IsNullOrWhiteSpace(country))
            return BadRequest("Country is required.");

        // Normaliserar inparametern (tar bort extra whitespace i början/slutet)
        var countryKey = country.Trim();

        var result = await _db.Friends
            // Filtrerar på:
            // - Address finns
            // - Country finns
            // - Country matchar exakt efter Trim()
            //
            // OBS: Detta blir oftast en case-sensitive/insensitive fråga beroende på DB-collation.
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country) &&
                        f.AddressDbM.Country.Trim() == countryKey)

            // Grupperar per stad i det landet:
            // - "-" om City saknas
            // - annars trim
            .GroupBy(f => string.IsNullOrWhiteSpace(f.AddressDbM!.City) ? "-" : f.AddressDbM!.City.Trim())

            // Skapar stads-översikten
            .Select(g => new
            {
                // Landet kommer från din routeparameter (countryKey)
                Country = countryKey,

                // Stadens nyckel
                City = g.Key,

                // Antal friends i staden
                FriendsCount = g.Count(),

                // Antal pets i staden:
                // SelectMany "flattar" alla pets för alla friends i gruppen och räknar dem.
                //
                // Kräver att PetsDbM är en navigation som EF känner till.
                // Om PetsDbM kan vara null (ovanligt om det initieras som tom lista) kan det annars ge problem.
                PetsCount = g.SelectMany(f => f.PetsDbM).Count()
            })

            // Sorterar på City A-Ö (så "-" hamnar ofta först)
            .OrderBy(x => x.City)
            .ToListAsync();

        return Ok(result);
    }
}

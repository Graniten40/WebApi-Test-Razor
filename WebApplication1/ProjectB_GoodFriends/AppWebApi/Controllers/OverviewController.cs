using DbContext;                 // MainDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWebApi.Controllers;

[ApiController]
[Route("api/overview")]
public class OverviewController : ControllerBase
{
    private readonly MainDbContext _db;

    public OverviewController(MainDbContext db)
    {
        _db = db;
    }

    // GET /api/overview/friends-by-country
    [HttpGet("friends-by-country")]
    public async Task<IActionResult> FriendsByCountry()
    {
        var result = await _db.Friends
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country))
            .GroupBy(f => f.AddressDbM!.Country.Trim())
            .Select(g => new
            {
                Country = g.Key,
                TotalFriends = g.Count(),
                Cities = g.Select(x => x.AddressDbM!.City)
                          .Where(c => !string.IsNullOrWhiteSpace(c))
                          .Select(c => c!.Trim())
                          .Distinct()
                          .Count()
            })
            .OrderByDescending(x => x.TotalFriends)
            .ToListAsync();

        return Ok(result);
    }

    // GET /api/overview/friends-by-country-city
    [HttpGet("friends-by-country-city")]
    public async Task<IActionResult> FriendsByCountryCity()
    {
        var result = await _db.Friends
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country))
            .GroupBy(f => new
            {
                Country = f.AddressDbM!.Country.Trim(),
                City = string.IsNullOrWhiteSpace(f.AddressDbM!.City) ? "-" : f.AddressDbM!.City.Trim()
            })
            .Select(g => new
            {
                Country = g.Key.Country,
                City = g.Key.City,
                NrFriends = g.Count()
            })
            .OrderBy(x => x.Country)
            .ThenByDescending(x => x.NrFriends)
            .ThenBy(x => x.City)
            .ToListAsync();

        return Ok(result);
    }

    // ✅ NEW: Overview Friends & Pets in the cities of a specific country
    // GET /api/overview/cities/{country}
    [HttpGet("cities/{country}")]
    public async Task<IActionResult> CitiesInCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return BadRequest("Country is required.");

        var countryKey = country.Trim();

        var result = await _db.Friends
            .Where(f => f.AddressDbM != null &&
                        !string.IsNullOrWhiteSpace(f.AddressDbM.Country) &&
                        f.AddressDbM.Country.Trim() == countryKey)
            .GroupBy(f => string.IsNullOrWhiteSpace(f.AddressDbM!.City) ? "-" : f.AddressDbM!.City.Trim())
            .Select(g => new
            {
                Country = countryKey,
                City = g.Key,
                FriendsCount = g.Count(),

                // Kräver att Friend har navigation: ICollection<PetDbM> PetsDbM (eller liknande).
                // Byt namnet PetsDbM nedan om din navigation heter något annat.
                PetsCount = g.SelectMany(f => f.PetsDbM).Count()
            })
            .OrderBy(x => x.City)
            .ToListAsync();

        return Ok(result);
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppRazor.Models;

namespace AppRazor.Services;

public class GoodFriendsApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public GoodFriendsApiClient(HttpClient http)
    {
        _http = http;
    }

    // ---------------------------
    // Guest / Info
    // ---------------------------
    public async Task<GuestInfoDto> GetGuestInfoAsync()
    {
        return await _http.GetFromJsonAsync<GuestInfoDto>("api/Guest/Info", JsonOptions)
               ?? new GuestInfoDto();
    }

    // ---------------------------
    // Admin / Seed
    // ---------------------------
    public async Task SeedAsync()
    {
        using var response = await _http.GetAsync("api/Admin/Seed");
        await EnsureSuccessWithBodyAsync(response);
    }

    public async Task RemoveSeedAsync()
    {
        using var response = await _http.GetAsync("api/Admin/RemoveSeed");
        await EnsureSuccessWithBodyAsync(response);
    }

    // ---------------------------
    // Friends / Read (paged list)
    // ---------------------------
    public async Task<ResponsePageDto<FriendListItemDto>> ReadFriendsAsync(
        bool seeded = false,
        bool flat = true,
        string? filter = null,
        int pageNr = 0,
        int pageSize = 50)
    {
        var url =
            $"api/Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
            $"&flat={flat.ToString().ToLowerInvariant()}" +
            $"&pageNr={pageNr}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(filter))
            url += $"&filter={Uri.EscapeDataString(filter)}";

        using var response = await _http.GetAsync(url);
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<ResponsePageDto<FriendListItemDto>>(JsonOptions)
               ?? new ResponsePageDto<FriendListItemDto>();
    }

    // ---------------------------------
    // Friends by Location (Country/City)
    // ---------------------------------
    public async Task<List<FriendListLocationDto>> ListFriendsAsync(
        string? country = null,
        string? city = null)
    {
        var url = "api/Friends/List";
        var qs = new List<string>();

        if (!string.IsNullOrWhiteSpace(country))
            qs.Add("country=" + Uri.EscapeDataString(country.Trim()));

        if (!string.IsNullOrWhiteSpace(city))
            qs.Add("city=" + Uri.EscapeDataString(city.Trim()));

        if (qs.Count > 0)
            url += "?" + string.Join("&", qs);

        using var response = await _http.GetAsync(url);
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<FriendListLocationDto>>(JsonOptions)
               ?? new List<FriendListLocationDto>();
    }

    // ---------------------------
    // Overview
    // ---------------------------
    public async Task<List<FriendsByCountryDto>> GetFriendsByCountryAsync()
    {
        using var response = await _http.GetAsync("api/overview/friends-by-country");
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<FriendsByCountryDto>>(JsonOptions)
               ?? new List<FriendsByCountryDto>();
    }

    public async Task<List<FriendsByCountryCityDto>> GetFriendsByCountryCityAsync()
    {
        using var response = await _http.GetAsync("api/overview/friends-by-country-city");
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<FriendsByCountryCityDto>>(JsonOptions)
               ?? new List<FriendsByCountryCityDto>();
    }

    public async Task<List<CityFriendsPetsOverviewDto>> GetCityFriendsPetsOverviewAsync(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return new List<CityFriendsPetsOverviewDto>();

        var url = $"api/overview/cities/{Uri.EscapeDataString(country.Trim())}";

        using var response = await _http.GetAsync(url);
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<CityFriendsPetsOverviewDto>>(JsonOptions)
               ?? new List<CityFriendsPetsOverviewDto>();
    }

    // ---------------------------
    // Quotes (Read + friendId) with server-filter + fallback
    // ---------------------------
    public async Task<IReadOnlyList<QuoteDto>> GetQuotesForFriendAsync(
        Guid friendId,
        bool seeded = false,
        bool flat = false,
        int pageNr = 0,
        int pageSize = 200)
    {
        // 1) ✅ Server-side filtering (same idea as Pets)
        var serverUrl =
            $"api/Quotes/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
            $"&flat={flat.ToString().ToLowerInvariant()}" +
            $"&pageNr={pageNr}&pageSize={pageSize}" +
            $"&friendId={Uri.EscapeDataString(friendId.ToString())}";

        using (var serverResponse = await _http.GetAsync(serverUrl))
        {
            await EnsureSuccessWithBodyAsync(serverResponse);

            var serverPage = await serverResponse.Content
                .ReadFromJsonAsync<ResponsePageDto<QuoteReadItemDto>>(JsonOptions)
                ?? new ResponsePageDto<QuoteReadItemDto>();

            var serverItems = serverPage.Items ?? Array.Empty<QuoteReadItemDto>();

            Console.WriteLine($"DEBUG Quotes server-filtered count={serverItems.Count} friend={friendId}");

            if (serverItems.Count > 0)
            {
                return serverItems
                    .Select(q => new QuoteDto
                    {
                        QuoteId = q.QuoteId,
                        Text = (q.QuoteText ?? "").Trim(),
                        Author = (q.Author ?? "").Trim()
                    })
                    .ToList();
            }
        }

        // 2) 🔁 Fallback: client-side filtering (if server filter is ignored/unsupported)
        var fallbackUrl =
            $"api/Quotes/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
            $"&flat={flat.ToString().ToLowerInvariant()}" +
            $"&pageNr=0&pageSize=500";

        using var fallbackResponse = await _http.GetAsync(fallbackUrl);
        await EnsureSuccessWithBodyAsync(fallbackResponse);

        var fallbackPage = await fallbackResponse.Content
            .ReadFromJsonAsync<ResponsePageDto<QuoteReadItemDto>>(JsonOptions)
            ?? new ResponsePageDto<QuoteReadItemDto>();

        var items = fallbackPage.Items ?? Array.Empty<QuoteReadItemDto>();

        bool MatchesFriend(QuoteReadItemDto q)
        {
            if (q.FriendId.HasValue && q.FriendId.Value == friendId) return true;
            if (q.FriendIds?.Contains(friendId) == true) return true;

            if (q.Friends?.Any(f =>
                    (f.FriendId.HasValue && f.FriendId.Value == friendId) ||
                    (f.Id.HasValue && f.Id.Value == friendId)
                ) == true)
                return true;

            return false;
        }

        var filtered = items
            .Where(MatchesFriend)
            .Select(q => new QuoteDto
            {
                QuoteId = q.QuoteId,
                Text = (q.QuoteText ?? "").Trim(),
                Author = (q.Author ?? "").Trim()
            })
            .ToList();

        Console.WriteLine($"DEBUG Quotes fallback count={items.Count} filtered={filtered.Count} friend={friendId}");

        return filtered;
    }

    // ---------------------------
    // Pets (Read + server filter friendId)
    // ---------------------------
    public async Task<IReadOnlyList<PetDto>> GetPetsForFriendAsync(
        Guid friendId,
        bool seeded = false,
        bool flat = false,
        int pageNr = 0,
        int pageSize = 200)
    {
        var url =
            $"api/Pets/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
            $"&flat={flat.ToString().ToLowerInvariant()}" +
            $"&pageNr={pageNr}&pageSize={pageSize}" +
            $"&friendId={Uri.EscapeDataString(friendId.ToString())}";

        using var response = await _http.GetAsync(url);
        await EnsureSuccessWithBodyAsync(response);

        var page =
            await response.Content.ReadFromJsonAsync<ResponsePageDto<PetReadApiItemDto>>(JsonOptions)
            ?? new ResponsePageDto<PetReadApiItemDto>();

        var items = page.Items ?? Array.Empty<PetReadApiItemDto>();

        Console.WriteLine($"DEBUG Pets server-filtered count={items.Count} friend={friendId}");

        return items
            .Select(p => new PetDto
            {
                PetId = p.PetId,
                Name = (p.PetName ?? p.Name ?? "").Trim(),
                Kind = (int)p.Kind,
                Mood = (int)p.Mood,
                StrKind = p.StrKind ?? "",
                StrMood = p.StrMood ?? ""
            })
            .ToList();
    }

    // ---------------------------
    // Friend / Details (NO flat=false här – den timeoutar i backend)
    // ---------------------------
    public async Task<FriendDetailsDto?> GetFriendDetailsAsync(Guid id)
    {
        // ✅ Lättare query: flat=true (ingen tung join)
        // seeded=false = “riktig” data (det du själv skapar)
        var friend = await FindFriendByIdLightAsync(id, seeded: true, pageSize: 50, maxPages: 20);
        if (friend == null)
            return null;

        // ✅ Hämta relationer separat (som pets redan funkar)
        friend.Pets   = (await GetPetsForFriendAsync(id, seeded: false, flat: false, pageNr: 0, pageSize: 200)).ToList();
        friend.Quotes = (await GetQuotesForFriendAsync(id, seeded: false, flat: false, pageNr: 0, pageSize: 200)).ToList();

        return friend;
    }

    // Hjälpmetod: bläddra i Friends/Read (flat=true) tills vi hittar rätt FriendId
    private async Task<FriendDetailsDto?> FindFriendByIdLightAsync(
        Guid id,
        bool seeded,
        int pageSize,
        int maxPages)
    {
        for (var pageNr = 0; pageNr < maxPages; pageNr++)
        {
            var url =
                $"api/Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
                $"&flat=true&pageNr={pageNr}&pageSize={pageSize}";

            using var response = await _http.GetAsync(url);
            await EnsureSuccessWithBodyAsync(response);

            var page = await response.Content.ReadFromJsonAsync<ResponsePageDto<FriendDetailsDto>>(JsonOptions)
                    ?? new ResponsePageDto<FriendDetailsDto>();

            var items = page.Items ?? Array.Empty<FriendDetailsDto>();

            var match = items.FirstOrDefault(f => f.FriendId == id);
            if (match != null)
                return match;

            // om API:t säger att det inte finns fler sidor
            if (items.Count < pageSize)
                break;
        }

        return null;
    }



    // ---------------------------
    // Friends / Update (Edit Friend + Address)
    // ---------------------------
    public async Task<ApiResult> UpdateFriendAsync(Guid friendId, FriendEditDto dto)
    {
        var request = new FriendUpdateRequestDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Birthday = dto.Birthday,
            Address = dto.Address is null ? null : new AddressUpdateRequestDto
            {
                StreetAddress = dto.Address.StreetAddress ?? "",
                ZipCode = dto.Address.ZipCode ?? 0,
                City = dto.Address.City ?? "",
                Country = dto.Address.Country ?? ""
            }
        };

        using var response = await _http.PutAsJsonAsync(
            $"api/Friends/UpdateItem/{friendId}",
            request,
            JsonOptions);

        if (response.IsSuccessStatusCode)
            return ApiResult.Success();

        if ((int)response.StatusCode == 400)
        {
            var parsed = await TryReadValidationErrorsAsync(response);
            if (parsed != null)
                return new ApiResult { Ok = false, ValidationErrors = parsed };

            var body = await response.Content.ReadAsStringAsync();
            return ApiResult.FromBadRequestBody(body);
        }

        var fallbackBody = await response.Content.ReadAsStringAsync();
        return ApiResult.Fail($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {fallbackBody}");
    }

    // ---------------------------
    // Pets / Create
    // ---------------------------
    public async Task AddPetAsync(Guid friendId, PetCreateDto dto)
    {
        var request = new
        {
            name = dto.Name,
            kind = (int)dto.Kind,
            mood = (int)dto.Mood,
            friendId = friendId
        };

        using var response = await _http.PostAsJsonAsync("api/Pets/CreateItem", request, JsonOptions);
        await EnsureSuccessWithBodyAsync(response);
    }

    // ---------------------------
    // Quotes / Create (robust: try friendIds, fallback friendId)
    // ---------------------------
    public async Task AddQuoteAsync(Guid friendId, QuoteCreateDto dto)
    {
        var text = (dto.Text ?? "").Trim();
        var author = (dto.Author ?? "").Trim();

        // Try 1: friendIds (many-to-many style)
        var request1 = new
        {
            quoteText = text,
            author = author,
            friendIds = new[] { friendId }
        };

        using (var resp1 = await _http.PostAsJsonAsync("api/Quotes/CreateItem", request1, JsonOptions))
        {
            if (resp1.IsSuccessStatusCode)
                return;

            // If it's a 400, try alternative payload
            if ((int)resp1.StatusCode != 400)
                await EnsureSuccessWithBodyAsync(resp1);

            var body1 = await resp1.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG AddQuote friendIds failed: {body1}");
        }

        // Try 2: friendId (singular style)
        var request2 = new
        {
            quoteText = text,
            author = author,
            friendId = friendId
        };

        using var resp2 = await _http.PostAsJsonAsync("api/Quotes/CreateItem", request2, JsonOptions);
        await EnsureSuccessWithBodyAsync(resp2);
    }

    // ---------------------------
    // Pets / Delete
    // ---------------------------
    public async Task DeletePetAsync(Guid petId)
    {
        using var response = await _http.DeleteAsync($"api/Pets/DeleteItem/{petId}");
        await EnsureSuccessWithBodyAsync(response);
    }

    // ---------------------------
    // Quotes / Delete
    // ---------------------------
    public async Task DeleteQuoteAsync(Guid quoteId)
    {
        using var response = await _http.DeleteAsync($"api/Quotes/DeleteItem/{quoteId}");
        await EnsureSuccessWithBodyAsync(response);
    }

    private static async Task<Dictionary<string, string[]>?> TryReadValidationErrorsAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemLike>(JsonOptions);
            if (problem?.Errors is null || problem.Errors.Count == 0)
                return null;

            return new Dictionary<string, string[]>(problem.Errors, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
    }

    // ---------------------------
    // Helpers
    // ---------------------------
    private static async Task EnsureSuccessWithBodyAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        var msg = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}";
        throw new HttpRequestException(msg);
    }

    // ---------------------------
    // Internal DTOs for Read endpoints
    // ---------------------------
    private sealed class QuoteReadItemDto
    {
        [JsonPropertyName("quoteId")]
        public Guid QuoteId { get; set; }

        [JsonPropertyName("quoteText")]
        public string? QuoteText { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("friends")]
        public List<FriendRefDto>? Friends { get; set; }

        [JsonPropertyName("friendIds")]
        public List<Guid>? FriendIds { get; set; }

        [JsonPropertyName("friendId")]
        public Guid? FriendId { get; set; }
    }

    private sealed class FriendRefDto
    {
        [JsonPropertyName("friendId")]
        public Guid? FriendId { get; set; }

        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
    }

    private sealed class PetReadApiItemDto
    {
        [JsonPropertyName("petId")]
        public Guid PetId { get; set; }

        [JsonPropertyName("petName")]
        public string? PetName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("kind")]
        public PetKind Kind { get; set; }

        [JsonPropertyName("mood")]
        public PetMood Mood { get; set; }

        [JsonPropertyName("strKind")]
        public string? StrKind { get; set; }

        [JsonPropertyName("strMood")]
        public string? StrMood { get; set; }
    }

    private sealed class ValidationProblemLike
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}

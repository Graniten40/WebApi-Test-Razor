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

/// <summary>
/// Thin HTTP client wrapper around the GoodFriends WebAPI.
/// Encapsulates:
/// - endpoint URLs
/// - JSON (de)serialization options
/// - consistent error handling (throwing HttpRequestException with body)
/// - small mapping/adaptation between API payloads and app DTOs
/// </summary>
public class GoodFriendsApiClient
{
    // HttpClient is injected via DI and configured in Program.cs (BaseAddress, headers, etc).
    private readonly HttpClient _http;

    /// <summary>
    /// Shared JSON options for all API calls.
    /// JsonSerializerDefaults.Web matches common ASP.NET Core Web defaults.
    /// PropertyNameCaseInsensitive makes it more forgiving if casing differs.
    /// </summary>
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

    /// <summary>
    /// Reads basic "guest info" from API.
    /// Returns an empty object if API returns null (keeps caller code simple).
    /// </summary>
    public async Task<GuestInfoDto> GetGuestInfoAsync()
    {
        return await _http.GetFromJsonAsync<GuestInfoDto>("api/Guest/Info", JsonOptions)
               ?? new GuestInfoDto();
    }

    // ---------------------------
    // Admin / Seed
    // ---------------------------

    /// <summary>
    /// Triggers API seed endpoint to populate test data.
    /// Uses EnsureSuccessWithBodyAsync for consistent error messages.
    /// </summary>
    public async Task SeedAsync()
    {
        using var response = await _http.GetAsync("api/Admin/Seed");
        await EnsureSuccessWithBodyAsync(response);
    }

    /// <summary>
    /// Removes previously seeded test data.
    /// </summary>
    public async Task RemoveSeedAsync()
    {
        using var response = await _http.GetAsync("api/Admin/RemoveSeed");
        await EnsureSuccessWithBodyAsync(response);
    }

    // ---------------------------
    // Friends / Read (paged list)
    // ---------------------------

    /// <summary>
    /// Reads friends as a paged list from API.
    /// - seeded controls whether seeded/test data is included
    /// - flat controls how heavy the API query is (joins/relations)
    /// - filter is a free-text search parameter supported by backend
    /// </summary>
    public async Task<ResponsePageDto<FriendListItemDto>> ReadFriendsAsync(
        bool seeded = false,
        bool flat = true,
        string? filter = null,
        int pageNr = 0,
        int pageSize = 50)
    {
        // Build querystring explicitly to keep URL predictable and debug-friendly
        var url =
            $"api/Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}" +
            $"&flat={flat.ToString().ToLowerInvariant()}" +
            $"&pageNr={pageNr}&pageSize={pageSize}";

        // Encode filter to avoid breaking querystring
        if (!string.IsNullOrWhiteSpace(filter))
            url += $"&filter={Uri.EscapeDataString(filter)}";

        using var response = await _http.GetAsync(url);
        await EnsureSuccessWithBodyAsync(response);

        // Fallback to an empty page object if deserialization returns null
        return await response.Content.ReadFromJsonAsync<ResponsePageDto<FriendListItemDto>>(JsonOptions)
               ?? new ResponsePageDto<FriendListItemDto>();
    }

    // ---------------------------------
    // Friends by Location (Country/City)
    // ---------------------------------

    /// <summary>
    /// Lists friends filtered by country and/or city.
    /// Adds querystring parameters only if the user provided them.
    /// </summary>
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

    /// <summary>
    /// Overview endpoint: number of friends grouped by country (summary table).
    /// </summary>
    public async Task<List<FriendsByCountryDto>> GetFriendsByCountryAsync()
    {
        using var response = await _http.GetAsync("api/overview/friends-by-country");
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<FriendsByCountryDto>>(JsonOptions)
               ?? new List<FriendsByCountryDto>();
    }

    /// <summary>
    /// Overview endpoint: number of friends grouped by country + city (details table).
    /// </summary>
    public async Task<List<FriendsByCountryCityDto>> GetFriendsByCountryCityAsync()
    {
        using var response = await _http.GetAsync("api/overview/friends-by-country-city");
        await EnsureSuccessWithBodyAsync(response);

        return await response.Content.ReadFromJsonAsync<List<FriendsByCountryCityDto>>(JsonOptions)
               ?? new List<FriendsByCountryCityDto>();
    }

    /// <summary>
    /// Drill-down endpoint: for a selected country, list cities + counts (friends + pets).
    /// Returns empty list if country is missing to avoid unnecessary API calls.
    /// </summary>
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

    /// <summary>
    /// Reads quotes for a friend.
    /// Strategy:
    /// 1) Try server-side filtering by friendId (preferred and efficient)
    /// 2) If that returns 0 items, fallback to client-side filtering (robust against API differences)
    /// Notes:
    /// - API payload shape differs between environments (friendId, friendIds, friends[]), hence MatchesFriend().
    /// </summary>
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

            // Read into internal DTO matching the Read endpoint schema
            var serverPage = await serverResponse.Content
                .ReadFromJsonAsync<ResponsePageDto<QuoteReadItemDto>>(JsonOptions)
                ?? new ResponsePageDto<QuoteReadItemDto>();

            var serverItems = serverPage.Items ?? Array.Empty<QuoteReadItemDto>();

            // Debug prints are useful during development to verify server filtering works
            Console.WriteLine($"DEBUG Quotes server-filtered count={serverItems.Count} friend={friendId}");

            // If the server supports friendId filtering, return the mapped items directly
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

        // Handles multiple possible API shapes for quote-to-friend relation
        bool MatchesFriend(QuoteReadItemDto q)
        {
            // Case 1: single friendId
            if (q.FriendId.HasValue && q.FriendId.Value == friendId) return true;

            // Case 2: list of friendIds
            if (q.FriendIds?.Contains(friendId) == true) return true;

            // Case 3: embedded friends references
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

    /// <summary>
    /// Reads pets for a friend using server-side filtering by friendId.
    /// Maps the API-specific read item DTO to app-level PetDto.
    /// </summary>
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

                // API can return either petName or name depending on version
                Name = (p.PetName ?? p.Name ?? "").Trim(),

                // In this app PetDto stores enums as ints (UI can show either int or string fields)
                Kind = (int)p.Kind,
                Mood = (int)p.Mood,

                // Optional "string versions" of enums from API
                StrKind = p.StrKind ?? "",
                StrMood = p.StrMood ?? ""
            })
            .ToList();
    }

    // ---------------------------
    // Friend / Details (NO flat=false här – den timeoutar i backend)
    // ---------------------------

    /// <summary>
    /// Loads a friend details view model.
    /// Important design choice:
    /// - We avoid heavy "flat=false" friends query because it can timeout in backend.
    /// - Instead: fetch friend "light" via paged list and then load relations (pets/quotes) separately.
    /// </summary>
    public async Task<FriendDetailsDto?> GetFriendDetailsAsync(Guid id)
    {
        // ✅ Light query: flat=true (avoid heavy joins)
        // NOTE: seeded flag should match where the friend exists (seeded test data vs "real" data)
        var friend = await FindFriendByIdLightAsync(id, seeded: true, pageSize: 50, maxPages: 20);
        if (friend == null)
            return null;

        // ✅ Load relations separately (smaller endpoints that are known to work)
        friend.Pets   = (await GetPetsForFriendAsync(id, seeded: false, flat: false, pageNr: 0, pageSize: 200)).ToList();
        friend.Quotes = (await GetQuotesForFriendAsync(id, seeded: false, flat: false, pageNr: 0, pageSize: 200)).ToList();

        return friend;
    }

    /// <summary>
    /// Helper: iterate Friends/Read pages (flat=true) until a matching FriendId is found.
    /// This is used as a workaround when the "heavy" details query is problematic.
    /// </summary>
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

            // Try to find the friend on this page
            var match = items.FirstOrDefault(f => f.FriendId == id);
            if (match != null)
                return match;

            // Stop early if API indicates there are no more pages
            if (items.Count < pageSize)
                break;
        }

        return null;
    }

    // ---------------------------
    // Friends / Update (Edit Friend + Address)
    // ---------------------------

    /// <summary>
    /// Updates a friend (and optional address).
    /// Converts Edit DTO to API request DTO (shape expected by backend).
    /// Returns ApiResult that can be used by Razor Page to show validation errors per field.
    /// </summary>
    public async Task<ApiResult> UpdateFriendAsync(Guid friendId, FriendEditDto dto)
    {
        // Build the request object expected by API update endpoint.
        // We do not send Address if it is null (optional address).
        var request = new FriendUpdateRequestDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Birthday = dto.Birthday,
            Address = dto.Address is null ? null : new AddressUpdateRequestDto
            {
                // Use empty strings / 0 to avoid nulls if API expects concrete values
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

        // Success case: no further parsing needed
        if (response.IsSuccessStatusCode)
            return ApiResult.Success();

        // 400 typically contains validation errors; try to parse them for field-level display
        if ((int)response.StatusCode == 400)
        {
            var parsed = await TryReadValidationErrorsAsync(response);
            if (parsed != null)
                return new ApiResult { Ok = false, ValidationErrors = parsed };

            // Fallback: parse generic bad request body (ProblemDetails-like or plain string)
            var body = await response.Content.ReadAsStringAsync();
            return ApiResult.FromBadRequestBody(body);
        }

        // Other status codes: return a single error string including body for debugging
        var fallbackBody = await response.Content.ReadAsStringAsync();
        return ApiResult.Fail($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {fallbackBody}");
    }

    // ---------------------------
    // Pets / Create
    // ---------------------------

    /// <summary>
    /// Creates a pet and associates it with a friend.
    /// Sends enums as ints because API expects numeric values for kind/mood.
    /// </summary>
    public async Task AddPetAsync(Guid friendId, PetCreateDto dto)
    {
        // Using anonymous type to match API CreateItem payload structure
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

    /// <summary>
    /// Creates a quote for a friend.
    /// Robust approach because API payload may differ:
    /// - Try friendIds (many-to-many style)
    /// - If 400 (bad request), fallback to friendId (singular style)
    /// </summary>
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

            // If it's not 400, treat as a real error and throw with body
            if ((int)resp1.StatusCode != 400)
                await EnsureSuccessWithBodyAsync(resp1);

            // If 400, log/debug the body and proceed to fallback payload
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

    /// <summary>
    /// Deletes a pet by id.
    /// </summary>
    public async Task DeletePetAsync(Guid petId)
    {
        using var response = await _http.DeleteAsync($"api/Pets/DeleteItem/{petId}");
        await EnsureSuccessWithBodyAsync(response);
    }

    // ---------------------------
    // Quotes / Delete
    // ---------------------------

    /// <summary>
    /// Deletes a quote by id.
    /// </summary>
    public async Task DeleteQuoteAsync(Guid quoteId)
    {
        using var response = await _http.DeleteAsync($"api/Quotes/DeleteItem/{quoteId}");
        await EnsureSuccessWithBodyAsync(response);
    }

    /// <summary>
    /// Attempts to parse ASP.NET Core-like validation errors:
    /// { "errors": { "Field": ["msg1","msg2"], ... } }
    /// Returns null if the response body is not in the expected format.
    /// </summary>
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

    /// <summary>
    /// Centralized HTTP error handling:
    /// throws HttpRequestException including status + reason + body.
    /// This makes Razor Page error messages much more informative during debugging.
    /// </summary>
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

    /// <summary>
    /// Internal DTO matching Quotes/Read response items.
    /// Kept private because it's only used for mapping in this client.
    /// </summary>
    private sealed class QuoteReadItemDto
    {
        [JsonPropertyName("quoteId")]
        public Guid QuoteId { get; set; }

        [JsonPropertyName("quoteText")]
        public string? QuoteText { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        // Some API variants include embedded friends
        [JsonPropertyName("friends")]
        public List<FriendRefDto>? Friends { get; set; }

        // Some API variants include a list of friend ids
        [JsonPropertyName("friendIds")]
        public List<Guid>? FriendIds { get; set; }

        // Some API variants include a single friend id
        [JsonPropertyName("friendId")]
        public Guid? FriendId { get; set; }
    }

    /// <summary>
    /// Internal DTO representing friend reference objects inside quote read items.
    /// Different API shapes may use "friendId" or "id".
    /// </summary>
    private sealed class FriendRefDto
    {
        [JsonPropertyName("friendId")]
        public Guid? FriendId { get; set; }

        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
    }

    /// <summary>
    /// Internal DTO matching Pets/Read response items.
    /// Some API versions return petName; others return name.
    /// </summary>
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

    /// <summary>
    /// Minimal model for ASP.NET Core validation problem details style.
    /// Used to parse "errors" from 400 Bad Request responses.
    /// </summary>
    private sealed class ValidationProblemLike
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}

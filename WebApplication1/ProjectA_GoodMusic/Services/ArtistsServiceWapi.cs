using System;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class ArtistsServiceWapi : IArtistsService
{
    private readonly ILogger<ArtistsServiceWapi> _logger;
    private readonly HttpClient _httpClient;

    //To ensure Json deserializern is using the class implementations instead of interfaces 
    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Converters = {
            new AbstractConverter<MusicGroup, IMusicGroup>(),
            new AbstractConverter<Album, IAlbum>(),
            new AbstractConverter<Artist, IArtist>()
        },
    };

    public ArtistsServiceWapi(IHttpClientFactory httpClientFactory, ILogger<ArtistsServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "MusicWebApi");
    }

    public async Task<ResponsePageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        string uri = $"artists/read?seeded={seeded}&flat={flat}&filter={filter}&pagenr={pageNumber}&pagesize={pageSize}";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the resonse data
        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponsePageDto<IArtist>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IArtist>> ReadArtistAsync(Guid id, bool flat)
    {
        string uri = $"artists/readitem?id={id}&flat={flat}";

        throw new NotImplementedException();
    }
    public async Task<ResponseItemDto<IArtist>> DeleteArtistAsync(Guid id)
    {
        string uri = $"artists/deleteitem/{id}";

        throw new NotImplementedException();
    }
    public async Task<ResponseItemDto<IArtist>> UpdateArtistAsync(ArtistCUdto item)
    {
        string uri = $"artists/updateitem/{item.ArtistId}";

        throw new NotImplementedException();
    }
    public async Task<ResponseItemDto<IArtist>> CreateArtistAsync(ArtistCUdto item)
    {
        string uri = $"artists/createitem";

        throw new NotImplementedException();
    }
}


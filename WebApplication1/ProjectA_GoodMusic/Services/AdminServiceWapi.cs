using System;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class AdminServiceWapi : IAdminService
{
    private readonly ILogger<AdminServiceWapi> _logger;
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

    public AdminServiceWapi(IHttpClientFactory httpClientFactory, ILogger<AdminServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "MusicWebApi");
    }
    
    public async Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync()
    {
        string uri = $"guest/info";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the response body
        string s = await response.Content.ReadAsStringAsync();
        var info = JsonConvert.DeserializeObject<ResponseItemDto<GstUsrInfoAllDto>>(s);
        return info;
    }


    public async Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        string uri = $"admin/seed?count={nrOfItems}";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the response body
        string s = await response.Content.ReadAsStringAsync();
        var info = JsonConvert.DeserializeObject<ResponseItemDto<GstUsrInfoAllDto>>(s);
        return info;
    }
    public async Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        string uri = $"admin/removeseed?seeded={seeded}";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the response body
        string s = await response.Content.ReadAsStringAsync();
        var info = JsonConvert.DeserializeObject<ResponseItemDto<GstUsrInfoAllDto>>(s);
        return info;
    }
}

public class AbstractConverter<TReal, TAbstract> : JsonConverter where TReal : TAbstract
{
    public override Boolean CanConvert(Type objectType)
        => objectType == typeof(TAbstract);

    public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        => serializer.Deserialize<TReal>(reader);

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        => serializer.Serialize(writer, value);
}


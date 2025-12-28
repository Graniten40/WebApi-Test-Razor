using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class MusicGroupsServiceWapi : IMusicGroupsService
{
    private readonly ILogger<MusicGroupsServiceWapi> _logger;
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

    public MusicGroupsServiceWapi(IHttpClientFactory httpClientFactory, ILogger<MusicGroupsServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "MusicWebApi");
    }

    public async Task<ResponsePageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) 
    {
        string uri = $"musicgroups/read?seeded={seeded}&flat={flat}&filter={filter}&pagenr={pageNumber}&pagesize={pageSize}";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the resonse data
        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponsePageDto<IMusicGroup>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IMusicGroup>> ReadMusicGroupAsync(Guid id, bool flat)
    {
        string uri = $"musicgroups/readitem?id={id}&flat={flat}";

        // Send request
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        // Throw if not successful
        await response.EnsureSuccessStatusMessage();

        // Read response body
        string s = await response.Content.ReadAsStringAsync();

        // Deserialize using interface->class converters
        var resp = JsonConvert.DeserializeObject<ResponseItemDto<IMusicGroup>>(s, _jsonSettings);

        return resp!;
    }

    public async Task<ResponseItemDto<IMusicGroup>> DeleteMusicGroupAsync(Guid id)
    {
        string uri = $"musicgroups/deleteitem/{id}";

        throw new NotImplementedException();
    }
    public async Task<ResponseItemDto<IMusicGroup>> UpdateMusicGroupAsync(MusicGroupCUdto item)
    {
        string uri = $"musicgroups/updateitem/{item.MusicGroupId}";

        throw new NotImplementedException();
    }
    public async Task<ResponseItemDto<IMusicGroup>> CreateMusicGroupAsync(MusicGroupCUdto item)
    {
        string uri = $"musicgroups/createitem";

        throw new NotImplementedException();
    }
}


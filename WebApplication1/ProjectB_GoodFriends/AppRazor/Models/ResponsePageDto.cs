using System.Text.Json.Serialization;

namespace AppRazor.Models;

public class ResponsePageDto<T>
{
    [JsonPropertyName("pageItems")]
    public IReadOnlyList<T> Items { get; init; } = [];

    public int PageNr { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages =>
        PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

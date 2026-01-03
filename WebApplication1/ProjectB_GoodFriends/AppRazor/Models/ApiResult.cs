using System.Text.Json;

namespace AppRazor.Models;

public class ApiResult
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public static ApiResult Success() => new() { Ok = true };
    public static ApiResult Fail(string error) => new() { Ok = false, Error = error };

    public static ApiResult FromBadRequestBody(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("errors", out var errorsEl))
            {
                var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

                foreach (var prop in errorsEl.EnumerateObject())
                {
                    var messages = prop.Value
                        .EnumerateArray()
                        .Select(x => x.GetString() ?? "")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                    dict[prop.Name] = messages;
                }

                return new ApiResult { Ok = false, ValidationErrors = dict };
            }
        }
        catch { }

        return Fail(string.IsNullOrWhiteSpace(body) ? "Bad request." : body);
    }
}

using System.Text.Json;

namespace AppRazor.Models;


// Standardiserat resultatobjekt för API-anrop.
// Används för att avgöra om ett anrop lyckades eller misslyckades,
// samt för att bära eventuella fel- eller valideringsmeddelanden.
public class ApiResult
{
   
    // Anger om API-anropet lyckades.
    public bool Ok { get; set; }
    
    // Generellt felmeddelande vid misslyckat anrop.
    public string? Error { get; set; }

    // Valideringsfel grupperade per fältnamn
    // (t.ex. från ASP.NET Core ModelState).
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    // Skapar ett lyckat API-resultat.
    public static ApiResult Success() => new() { Ok = true };

    // Skapar ett misslyckat API-resultat med ett felmeddelande.
    public static ApiResult Fail(string error) =>
        new() { Ok = false, Error = error };

    // Försöker tolka ett BadRequest-svar (JSON) från API:t
    // och extraherar eventuella valideringsfel.
    public static ApiResult FromBadRequestBody(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);

            // ASP.NET Core skickar ofta valideringsfel under nyckeln "errors"
            if (doc.RootElement.TryGetProperty("errors", out var errorsEl))
            {
                var dict = new Dictionary<string, string[]>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (var prop in errorsEl.EnumerateObject())
                {
                    var messages = prop.Value
                        .EnumerateArray()
                        .Select(x => x.GetString() ?? "")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                    dict[prop.Name] = messages;
                }

                return new ApiResult
                {
                    Ok = false,
                    ValidationErrors = dict
                };
            }
        }
        catch
        {
            // Om JSON inte kan tolkas faller vi tillbaka till ett generellt fel
        }

        return Fail(
            string.IsNullOrWhiteSpace(body)
                ? "Bad request."
                : body
        );
    }
}

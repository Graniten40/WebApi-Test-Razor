using System.Text.Json.Serialization;

namespace AppRazor.Models;

public class FriendDetailsDto
{
    public Guid FriendId { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string Email     { get; set; } = "";

    public DateTime? Birthday { get; set; }

    public AddressDto? Address { get; set; }
    public List<PetDto> Pets { get; set; } = new();
    public List<QuoteDto> Quotes { get; set; } = new();

    public string Name => $"{FirstName} {LastName}".Trim();
}

public class AddressDto
{
    public Guid AddressId { get; set; }

    public string? StreetAddress { get; set; } 
    public int ZipCode { get; set; }
    public string? City { get; set; } 
    public string? Country { get; set; } 
}

public class PetDto
{
    public Guid PetId { get; set; }

    public string Name { get; set; } = "";
    public int Kind { get; set; }
    public int Mood { get; set; }

    public string? StrKind { get; set; }
    public string? StrMood { get; set; }
}

public class QuoteDto
{
    public Guid QuoteId { get; set; }

    [JsonPropertyName("quoteText")]
    public string Text { get; set; } = "";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "";
}



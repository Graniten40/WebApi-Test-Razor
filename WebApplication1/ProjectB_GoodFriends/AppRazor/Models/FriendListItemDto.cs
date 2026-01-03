using System;
using System.Text.Json.Serialization;

namespace AppRazor.Models;

public class FriendListItemDto
{
    [JsonPropertyName("friendId")]
    public Guid FriendId { get; init; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("seeded")]
    public bool Seeded { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

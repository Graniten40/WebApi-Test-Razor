using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppRazor.Models;

public class PetReadItemDto
{
    [JsonPropertyName("petId")]
    public Guid PetId { get; set; }

    // API kan skicka name eller petName – vi tar båda
    [JsonPropertyName("petName")]
    public string? PetName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("kind")]
    public PetKind Kind { get; set; }

    [JsonPropertyName("mood")]
    public PetMood Mood { get; set; }

    // Viktigt: relationen kan skickas som friendId, friendIds eller friends
    [JsonPropertyName("friendId")]
    public Guid? FriendId { get; set; }

    [JsonPropertyName("friendIds")]
    public List<Guid>? FriendIds { get; set; }

    [JsonPropertyName("friends")]
    public List<FriendListItemDto>? Friends { get; set; }

    [JsonPropertyName("strKind")]
    public string? StrKind { get; set; }

    [JsonPropertyName("strMood")]
    public string? StrMood { get; set; }
}

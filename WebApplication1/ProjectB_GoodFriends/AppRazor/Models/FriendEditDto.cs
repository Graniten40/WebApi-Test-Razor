using System.ComponentModel.DataAnnotations;

namespace AppRazor.Models;

public class FriendEditDto
{
    [Required]
    public Guid FriendId { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    public DateTime? Birthday { get; set; }

    // Adress kan vara optional – men om du vill validera fält när den finns:
    public AddressEditDto? Address { get; set; }
}

public class AddressEditDto
{
    public Guid AddressId { get; set; }

    [StringLength(100)]
    public string? StreetAddress { get; set; }

    [Range(0, 99999)]
    public int? ZipCode { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }
}


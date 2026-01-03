using System.ComponentModel.DataAnnotations;

namespace Models.DTO;

public class FriendUpdateRequestDto
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    public DateTime? Birthday { get; set; }

    public AddressUpdateRequestDto? Address { get; set; }
}

public class AddressUpdateRequestDto
{
    [StringLength(100)]
    public string StreetAddress { get; set; } = "";

    [Range(0, 99999)]
    public int ZipCode { get; set; }

    [StringLength(100)]
    public string City { get; set; } = "";

    [StringLength(100)]
    public string Country { get; set; } = "";
}

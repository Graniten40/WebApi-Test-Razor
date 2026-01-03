using System;
using System.ComponentModel.DataAnnotations;

namespace AppWebApi.Dtos;

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

    public AddressEditDto? Address { get; set; }
}

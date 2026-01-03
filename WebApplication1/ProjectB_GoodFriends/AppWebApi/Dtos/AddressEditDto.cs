using System;
using System.ComponentModel.DataAnnotations;

namespace AppWebApi.Dtos;

public class AddressEditDto
{
    public Guid AddressId { get; set; }

    [StringLength(100)]
    public string StreetAddress { get; set; } = "";

    [Range(0, 99999)]
    public int ZipCode { get; set; }

    [StringLength(100)]
    public string City { get; set; } = "";

    [StringLength(100)]
    public string Country { get; set; } = "";
}

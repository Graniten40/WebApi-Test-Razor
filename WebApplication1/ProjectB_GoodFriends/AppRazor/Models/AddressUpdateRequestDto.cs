namespace AppRazor.Models;

public class AddressUpdateRequestDto
{
    public string StreetAddress { get; set; } = "";
    public int ZipCode { get; set; }
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
}

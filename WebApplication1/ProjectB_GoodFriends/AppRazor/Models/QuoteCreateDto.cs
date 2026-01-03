using System.ComponentModel.DataAnnotations;

namespace AppRazor.Models;

public class QuoteCreateDto
{
    [Required, StringLength(300)]
    public string Text { get; set; } = "";

    [Required, StringLength(100)]
    public string Author { get; set; } = "";
}

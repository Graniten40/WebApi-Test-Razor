using System.ComponentModel.DataAnnotations;

namespace AppRazor.Models;

public class PetCreateDto
{
    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    public PetKind Kind { get; set; }
    public PetMood Mood { get; set; }
}



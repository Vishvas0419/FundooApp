using System.ComponentModel.DataAnnotations;

namespace Models;

public class EditNoteModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Colour { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepositoryLayer.Entity;


//this class members represent a row which will added to the Users tables
public class UserEntity
{
    [Key]
    [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
    public int UserId { get; set; }
    [Required] 
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    
}
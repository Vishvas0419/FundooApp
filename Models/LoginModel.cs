using System.ComponentModel.DataAnnotations;

namespace Models;

public class LoginModel
{
    [RegularExpression(@"^[a-zA-Z0-9_%]{1,22}@[a-zA-Z0-9]{1,22}\.[a-zA-Z0-9]{1,22}")]
    [Required]
    public string Email { get; set; }
    
    public string Password { get; set; }
    
    
}
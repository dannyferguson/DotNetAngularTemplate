using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Models.DTO;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Password must be between 12 and 128 characters.")]
    public string Password { get; set; }
}
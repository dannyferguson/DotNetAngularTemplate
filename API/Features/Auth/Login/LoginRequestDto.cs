using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Features.Auth.Login;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [StringLength(128, ErrorMessage = "Password must not be longer than 128 characters.")]
    public string Password { get; set; } = null!;
}
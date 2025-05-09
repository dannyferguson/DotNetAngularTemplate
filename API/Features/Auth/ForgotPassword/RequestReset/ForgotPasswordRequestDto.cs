using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
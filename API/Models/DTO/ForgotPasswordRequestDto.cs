using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Models.DTO;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Models.DTO;

public class ForgotEmailRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
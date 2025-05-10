using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Features.Profile.UpdateEmail;

public class UpdateEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
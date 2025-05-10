using System.ComponentModel.DataAnnotations;

namespace DotNetAngularTemplate.Features.Auth.ConfirmEmail;

public class EmailConfirmationRequestDto
{
    [Required] public string Code { get; set; } = null!;
}
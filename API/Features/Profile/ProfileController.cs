using System.Security.Claims;
using DotNetAngularTemplate.Extensions;
using DotNetAngularTemplate.Features.Auth;
using DotNetAngularTemplate.Features.Auth.ConfirmEmail;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.ConfirmReset;
using DotNetAngularTemplate.Features.Auth.ForgotPassword.RequestReset;
using DotNetAngularTemplate.Features.Auth.Login;
using DotNetAngularTemplate.Features.Auth.Register;
using DotNetAngularTemplate.Features.Profile.UpdateEmail;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Models;
using JasperFx.Core;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;

namespace DotNetAngularTemplate.Features.Profile;

[ApiController]
[Route("api/v1/profile")]
[EnableRateLimiting("AuthPolicy")]
[AutoValidateAntiforgeryToken]
[Authorize]
public class ProfileController(
    ILogger<AuthController> logger,
    IMessageBus bus) : ControllerBase
{
    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto requestDto,
        CancellationToken cancellationToken)
    {
        var ip = IpHelper.GetClientIp(HttpContext);

        // var result =
        //     await bus.InvokeAsync<ApiResult>(new CreateUserCommand(ip, requestDto.Email, requestDto.Password,
        //         cancellationToken), cancellationToken);
        //
        // return result.IsSuccess ? Ok(result) : StatusCode(StatusCodes.Status500InternalServerError, result);
        return Ok();
    }
    
    [HttpGet("current")]
    public IActionResult Current()
    {
        var userId = User.GetUserId();
        var email = User.GetEmail();

        return Ok(ApiResultWithValue<Profile>.Success(new Profile(userId, email)));
    }
}
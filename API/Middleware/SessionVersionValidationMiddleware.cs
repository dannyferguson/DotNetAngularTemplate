using DotNetAngularTemplate.Infrastructure.Services;
using System.Security.Claims;

namespace DotNetAngularTemplate.Middleware;

public class SessionVersionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, SessionVersionService sessionVersionService)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var versionClaim = user.FindFirst("SessionVersion")?.Value;

            if (userId != null && versionClaim != null)
            {
                var currentVersion = await sessionVersionService.GetVersionAsync(userId);

                if (versionClaim != currentVersion)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Session invalidated.");
                    return;
                }
            }
        }

        await next(context);
    }
}
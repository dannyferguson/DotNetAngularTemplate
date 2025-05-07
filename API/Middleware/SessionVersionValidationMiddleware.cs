using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Authentication;

namespace DotNetAngularTemplate.Middleware;

public class SessionVersionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserSessionVersionService sessionService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var valid = await sessionService.IsSessionValid(context);

            if (!valid)
            {
                await context.SignOutAsync("AppCookie");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Session invalidated.");
                return;
            }
        }

        await next(context);
    }
}
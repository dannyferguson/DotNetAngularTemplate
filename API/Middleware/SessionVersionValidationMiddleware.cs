using DotNetAngularTemplate.Services;

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
                context.Session.Clear();
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Session invalidated.");
                return;
            }
        }

        await next(context);
    }
}

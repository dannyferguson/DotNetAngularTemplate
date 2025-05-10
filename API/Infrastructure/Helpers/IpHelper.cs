namespace DotNetAngularTemplate.Infrastructure.Helpers;

public static class IpHelper
{
    public static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = forwardedFor?.Split(',').FirstOrDefault()?.Trim()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return ip;
    }
}
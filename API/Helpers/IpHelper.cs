namespace DotNetAngularTemplate.Helpers;

public static class IpHelper
{
    public static string GetClientIp(HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
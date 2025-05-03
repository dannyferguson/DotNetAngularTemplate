using System.Security.Cryptography;

namespace DotNetAngularTemplate.Middleware;

public class CspNonceMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var nonceBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonceBytes);
        var nonce = Convert.ToBase64String(nonceBytes);
        context.Items["CSPNonce"] = nonce;

        context.Response.Headers["Content-Security-Policy"] =
            $"default-src 'self'; " +
            $"style-src 'self' 'nonce-{nonce}'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            $"object-src 'none'; img-src 'self' https://tailwindcss.com https://images.unsplash.com;";

        await next(context);
    }
}
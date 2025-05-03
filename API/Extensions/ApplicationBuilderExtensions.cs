using DotNetAngularTemplate.Middleware;

namespace DotNetAngularTemplate.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static IApplicationBuilder UseCspNonce(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CspNonceMiddleware>();
    }

    public static IApplicationBuilder UseIndexHtmlNonceInjection(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IndexHtmlNonceInjectionMiddleware>();
    }
}

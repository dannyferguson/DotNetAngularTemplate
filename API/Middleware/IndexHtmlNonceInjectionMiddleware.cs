namespace DotNetAngularTemplate.Middleware;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

public class IndexHtmlNonceInjectionMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    // Define a HashSet for quick lookup of static file extensions
    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico",
        ".woff", ".woff2", ".ttf", ".eot", ".map", ".json", ".webp", ".avif"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        // Check if the request is not for an API endpoint and does not target a static file
        bool isApi = path.StartsWithSegments("/api");
        bool isStatic = Path.HasExtension(path) && ExcludedExtensions.Contains(Path.GetExtension(path));

        if (!isApi && !isStatic)
        {
            var file = Path.Combine(env.WebRootPath, "browser/index.html");

            if (File.Exists(file))
            {
                var html = await File.ReadAllTextAsync(file);
                var nonce = context.Items["CSPNonce"]?.ToString() ?? "";
                html = html.Replace("CSP_NONCE_PLACEHOLDER", nonce);
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
                return;
            }
        }

        await next(context);
    }
}
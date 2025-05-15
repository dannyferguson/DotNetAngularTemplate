using DotNetAngularTemplate.Extensions;
using DotNetAngularTemplate.Filters;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppCqrs();
builder.Services.AddAuthenticationAndAuthorization();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});
builder.Services.AddAppRedisCache(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddAppAntiforgery();
builder.Services.AddOpenApi();
builder.Services.AddMysqlDatabaseService(builder.Configuration);
builder.Services.AddResendEmailing(builder.Configuration);
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddSingleton<SessionVersionService>();

var app = builder.Build();

// Ensure app can connect to MySQL and Redis
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("StartupHealthCheck");
    
    if (!await StartupHealthCheck.CheckCriticalServicesAsync(scope.ServiceProvider))
    {
        logger.LogCritical("Startup health check (MySQL + Redis connection) failed.");
        return;
    }
    
    logger.LogInformation("Startup health check (MySQL + Redis connection) passed!");
}

// Setup middleware
app.UseSecurityHeaders();
app.UseCspNonce();
app.UseIndexHtmlNonceInjection();
app.UseGetEmailFromRequest();
app.UseSessionVersionValidation();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "browser")),
    RequestPath = ""
});
app.UseAuthentication(); 
app.UseAuthorization();
app.UseRateLimiter();

// Map controllers and everything else to static (Angular) files
app.MapControllers();
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapFallbackToFile("/browser/index.html");
    });
});

// Start App
app.Run();
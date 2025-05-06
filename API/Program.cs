using DotNetAngularTemplate.Extensions;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Register essential services
builder.Services.AddControllersWithViews();
builder.Services.AddAppRedisCache(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddAppAntiforgery();
builder.Services.AddOpenApi();
builder.Services.AddMysqlDatabaseService(builder.Configuration);
builder.Services.AddResendEmailing(builder.Configuration);

// Register other services
builder.Services.AddScoped<AuthService>();
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

var app = builder.Build();

// Setup middleware
app.UseSecurityHeaders();
app.UseCspNonce();
app.UseIndexHtmlNonceInjection();
app.UseGetEmailFromRequest();

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
app.UseSession();
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
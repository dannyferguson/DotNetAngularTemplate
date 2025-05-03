using System.Threading.RateLimiting;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add redis cache + sessions
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (redisConnectionString == null)
{
    Console.WriteLine(
        "Missing environment variable ConectionStrings__Redis. Please set it before running the application!");
    Environment.Exit(1);
}

builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromDays(7);
});

// Add MySQL support/connection
var mysqlConnectionString = builder.Configuration.GetConnectionString("Default");
if (mysqlConnectionString == null)
{
    Console.WriteLine(
        "Missing environment variable ConectionStrings__Default. Please set it before running the application!");
    Environment.Exit(1);
}

builder.Services.AddSingleton<DatabaseService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
    return new DatabaseService(logger, mysqlConnectionString);
});
builder.Services.AddSingleton<AuthService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

// Disable automatic error 400 formatting
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

// Enable rate-limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = IpHelper.GetClientIp(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });


    options.AddPolicy("AuthPolicy", httpContext =>
    {
        var ip = IpHelper.GetClientIp(httpContext);
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                AutoReplenishment = true
            });
    });
        
    
    options.RejectionStatusCode =  StatusCodes.Status429TooManyRequests;
});

// Add CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; 
});

var app = builder.Build();

// Configure basic security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff"; // Prevent MIME type sniffing
    context.Response.Headers["X-Frame-Options"] = "DENY"; // Prevent clickjacking
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block"; // Legacy browsers only
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin"; // Limit referrer leakage
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=(), fullscreen=(self)"; // Limit APIs
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; object-src 'none'"; // Only allow same origin content/scripts

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseHsts();

// Serve Angular from wwwroot folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "browser")),
    RequestPath = ""
});

app.UseSession();
app.UseRateLimiter();
app.MapControllers();

// Map all requests not at /api to Angular
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
    {
        appBuilder.UseRouting();
        appBuilder.UseEndpoints(endpoints => { endpoints.MapFallbackToFile("/browser/index.html"); });
    }
);

app.Run();
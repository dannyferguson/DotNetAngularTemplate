using System.Threading.RateLimiting;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Services;
using Resend;
using StackExchange.Redis;

namespace DotNetAngularTemplate.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppRedisCache(this IServiceCollection services, IConfiguration config)
    {
        var redisConnectionString = config.GetConnectionString("Redis");
        if (redisConnectionString == null)
        {
            Console.WriteLine("Missing environment variable ConectionStrings__Redis. Please set it before running the application!");
            Environment.Exit(1);
        }

        services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
        
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.IdleTimeout = TimeSpan.FromDays(7);
        });

        return services;
    }

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global policy of 100 requests per minute. This should be more than enough for regular usage of the application.
            // Limit: 100 requests per minute per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ip = IpHelper.GetClientIp(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                });
            });

            // Policy for most auth related endpoints that's a bit stricter than the global one to limit brute forcing.
            // Limit: 10 requests per minute per IP
            options.AddPolicy("AuthPolicy", httpContext =>
            {
                var ip = IpHelper.GetClientIp(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true
                });
            });
            
            // Log rate limit hits for auditing and set response code to 429
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                
                var ip = IpHelper.GetClientIp(context.HttpContext);
                var path = context.HttpContext.Request.Path;
                var time = DateTime.UtcNow;

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiter");

                logger.LogWarning("Rate limit exceeded. IP: {IP}, Path: {Path}, Time: {TimeUtc}",
                    ip, path, time);
            };
        });

        return services;
    }

    public static IServiceCollection AddAppAntiforgery(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
        });

        return services;
    }

    public static IServiceCollection AddMysqlDatabaseService(this IServiceCollection services, IConfiguration config)
    {
        var mysqlConnectionString = config.GetConnectionString("Default");
        if (mysqlConnectionString == null)
        {
            Console.WriteLine("Missing environment variable ConectionStrings__Default. Please set it before running the application!");
            Environment.Exit(1);
        }
        
        services.AddSingleton<DatabaseService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
            return new DatabaseService(logger, mysqlConnectionString);
        });

        return services;
    }

    public static IServiceCollection AddResendEmailing(this IServiceCollection services, IConfiguration config)
    {
        var apiKey = config.GetSection("Emails:ResendApiKey").Value;
        var emailFrom = config.GetSection("Emails:From").Value;
        if (apiKey == null || emailFrom == null)
        {
            if (apiKey == null)
            {
                Console.WriteLine("Missing environment variable Emails__ResendApiKey. Please set it before running the application!");
            }

            if (emailFrom == null)
            {
                Console.WriteLine("Missing environment variable Emails__From. Please set it before running the application!");
            }

            Environment.Exit(1);
        }
        
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>( o =>
        {
            o.ApiToken = apiKey;
        } );
        services.AddScoped<IResend, ResendClient>();
        services.AddScoped<EmailService>();
        
        // Add custom email rate limiting service to prevent over-spend
        services.AddSingleton<EmailRateLimitService>();

        return services;
    }
}
using System.Threading.RateLimiting;
using DotNetAngularTemplate.Helpers;
using DotNetAngularTemplate.Services;

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

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
}
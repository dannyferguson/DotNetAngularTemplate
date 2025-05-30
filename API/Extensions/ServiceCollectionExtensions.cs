﻿using System.Threading.RateLimiting;
using DotNetAngularTemplate.Infrastructure.CQRS;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Services;
using Resend;
using StackExchange.Redis;

namespace DotNetAngularTemplate.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication("AppCookie")
            .AddCookie("AppCookie", options =>
            {
                options.Cookie.Name = ".Auth.Cookie";
                options.LoginPath = "/noop";
                options.AccessDeniedPath = "/noop";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);

                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });
        
        services.AddAuthorization();
        
        return services;
    }
    
    public static IServiceCollection AddAppRedisCache(this IServiceCollection services, IConfiguration config)
    {
        var redisConnectionString = config.GetConnectionString("Redis");
        if (redisConnectionString == null)
        {
            Console.WriteLine("Missing environment variable ConnectionStrings__Redis. Please set it before running the application!");
            Environment.Exit(1);
        }

        services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
        
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
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
            Console.WriteLine("Missing environment variable ConnectionStrings__Default. Please set it before running the application!");
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
    
    public static IServiceCollection AddAppCqrs(this IServiceCollection services)
    {
        services.AddScoped<Mediator>();

        var assembly = typeof(Mediator).Assembly;

        var handlerTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}
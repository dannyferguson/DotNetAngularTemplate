using DotNetAngularTemplate.Infrastructure.Services;
using StackExchange.Redis;

namespace DotNetAngularTemplate.Infrastructure.Helpers;

public static class StartupHealthCheck
{
    public static async Task<bool> CheckCriticalServicesAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<DatabaseService>();
        if (!await db.TestConnectionAsync())
        {
            Console.WriteLine("Unable to connect to MySQL. Shutting down.");
            return false;
        }

        var redis = services.GetRequiredService<IConnectionMultiplexer>();
        try
        {
            await redis.GetDatabase().PingAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to connect to Redis. Shutting down.");
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }
}
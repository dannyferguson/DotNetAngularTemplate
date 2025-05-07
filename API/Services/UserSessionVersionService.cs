using StackExchange.Redis;
using System.Security.Claims;

namespace DotNetAngularTemplate.Services;

public class UserSessionVersionService(
    ILogger<UserSessionVersionService> logger,
    IConnectionMultiplexer multiplexer)
{
    private readonly IDatabase _redisDb = multiplexer.GetDatabase();
    private const string RedisKeyPrefix = "session-version:";

    public async Task<string> GetVersionAsync(string userId)
    {
        var version = await _redisDb.StringGetAsync($"{RedisKeyPrefix}{userId}");
        return version.IsNullOrEmpty ? "1" : version!;
    }

    public async Task BumpVersionAsync(string userId)
    {
        await _redisDb.StringIncrementAsync($"{RedisKeyPrefix}{userId}");
        logger.LogInformation("Bumped session version for user {UserId}", userId);
    }

    public void SetVersionInSession(HttpContext context, string version)
    {
        context.Session.SetString("SessionVersion", version);
    }

    public string? GetVersionFromSession(HttpContext context)
    {
        return context.Session.GetString("SessionVersion");
    }

    public string? GetUserIdFromClaims(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<bool> IsSessionValid(HttpContext context)
    {
        var userId = GetUserIdFromClaims(context);
        Console.WriteLine($"id={userId}");
        if (string.IsNullOrWhiteSpace(userId)) return true;

        var sessionVersion = GetVersionFromSession(context);
        var currentVersion = await GetVersionAsync(userId);

        return sessionVersion == currentVersion;
    }
}
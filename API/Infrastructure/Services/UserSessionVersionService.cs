using StackExchange.Redis;
using System.Security.Claims;

namespace DotNetAngularTemplate.Services;

public class UserSessionVersionService(
    ILogger<UserSessionVersionService> logger,
    IConnectionMultiplexer multiplexer)
{
    private readonly IDatabase _redisDb = multiplexer.GetDatabase();
    private const string RedisKeyPrefix = "session-version:";
    private const string ClaimTypeSessionVersion = "SessionVersion";

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

    public string? GetUserIdFromClaims(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetSessionVersionFromClaims(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypeSessionVersion)?.Value;
    }

    public async Task<bool> IsSessionValid(HttpContext context)
    {
        var userId = GetUserIdFromClaims(context);
        if (string.IsNullOrWhiteSpace(userId)) return true;

        var versionFromClaim = GetSessionVersionFromClaims(context);
        var currentVersion = await GetVersionAsync(userId);

        return versionFromClaim == currentVersion;
    }

    public Claim CreateVersionClaim(string version)
    {
        return new Claim(ClaimTypeSessionVersion, version);
    }
}
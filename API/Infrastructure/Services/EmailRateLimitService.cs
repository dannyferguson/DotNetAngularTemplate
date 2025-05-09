using StackExchange.Redis;

namespace DotNetAngularTemplate.Services;

public class EmailRateLimitService(ILogger<EmailRateLimitService> logger, IConnectionMultiplexer multiplexer)
{
    private readonly IDatabase _redisDb = multiplexer.GetDatabase();
    private readonly TimeSpan _limitWindow = TimeSpan.FromHours(1);
    private readonly int _maxHits = 3;

    public async Task<bool> CanSendAsync(string key)
    {
        key = key.Trim().ToLowerInvariant();
        
        var count = await _redisDb.StringIncrementAsync(key);

        if (count == 1)
        {
            // Set expiry on first hit
            await _redisDb.KeyExpireAsync(key, _limitWindow);
        }

        if (count > _maxHits)
        {
            logger.LogWarning("Email rate limit hit on key {key}", key);
        }

        return count <= _maxHits;
    }
}
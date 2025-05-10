using System.Security.Claims;
using DotNetAngularTemplate.Infrastructure.Models;
using MySqlConnector;

namespace DotNetAngularTemplate.Infrastructure.Services;

using StackExchange.Redis;
using Microsoft.Extensions.Logging;

public class SessionVersionService(
    IConnectionMultiplexer multiplexer,
    ILogger<SessionVersionService> logger,
    DatabaseService databaseService)
{
    private readonly IDatabase _redisDb = multiplexer.GetDatabase();

    private const string RedisKeyPrefix = "session-version:";
    private const string ClaimTypeSessionVersion = "SessionVersion";

    public async Task<string> GetVersionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = RedisKeyPrefix + userId;
        var version = await _redisDb.StringGetAsync(key);
        if (!version.IsNullOrEmpty)
        {
            return version!;
        }

        var dbVersion = await GetVersionFromDbAsync(userId, cancellationToken);
        _ = _redisDb.StringSetAsync(key, dbVersion, TimeSpan.FromMinutes(30));
        return dbVersion;
    }

    private async Task<string> GetVersionFromDbAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT session_version FROM users WHERE id = @Id";
        var parameters = new Dictionary<string, object>
        {
            ["@Id"] = userId
        };

        var version = await databaseService.QuerySingleAsync<int?>(
            sql,
            parameters,
            reader => reader.GetInt32(0),
            cancellationToken);

        return (version?.ToString() ?? "1");
    }

    public async Task<ApiResult> BumpVersionAsync(string userId, DatabaseUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = "UPDATE users SET session_version = session_version + 1 WHERE id = 1";
            var parameters = new Dictionary<string, object>
            {
                ["@UserId"] = userId
            };
            await unitOfWork.ExecuteAsync(sql, parameters, cancellationToken);

            await _redisDb.StringIncrementAsync(RedisKeyPrefix + userId);
            
            logger.LogInformation("Successfully bumped session version for user with id '{UserId}'.", userId);
            return ApiResult.Success();
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex, "Error during bumping of session version for user with id '{UserId}'. SQL State: {ExSqlState}, Error Code: {ExNumber}", userId, ex.SqlState, ex.Number);
            return ApiResult.Failure();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bump session version for user with id {UserId}", userId);
            return ApiResult.Failure();
        }
    }
    
    public Claim CreateVersionClaim(string version)
    {
        return new Claim(ClaimTypeSessionVersion, version);
    }
}


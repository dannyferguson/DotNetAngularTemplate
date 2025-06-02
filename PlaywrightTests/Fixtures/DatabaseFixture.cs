using System.Security.Cryptography;
using DotNetAngularTemplate.Infrastructure.Helpers;
using DotNetAngularTemplate.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using PlaywrightTests.Config;

namespace PlaywrightTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly TestConfig _config = TestConfig.Load();
    private readonly Dictionary<string, string> _testUsersToDispose = new();

    public DatabaseService Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? throw new InvalidOperationException("ConnectionStrings__Default environment variable not set");
        
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });
        });

        var logger = loggerFactory.CreateLogger<DatabaseService>();
            
        Database = new DatabaseService(logger, connectionString);
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await using var unitOfWork = await Database.BeginUnitOfWorkAsync();
        
        foreach (var testUserPair in _testUsersToDispose)
        {
            await unitOfWork.ExecuteAsync("DELETE FROM users WHERE email = @Email", new()
            {
                ["@Email"] = testUserPair.Key
            });
        }
        
        await unitOfWork.CommitAsync();
    }

    public async Task<(string, string)> GenerateAccount(bool emailVerified)
    {
        var credentials = GenerateTestCredentials();
        var passwordHash = PasswordHelper.HashPassword(credentials.Item2);

        await using var unitOfWork = await Database.BeginUnitOfWorkAsync();

        const string sql =
            "INSERT INTO users (email, email_verified, password_hash, created_at, updated_at) VALUES (@Email, @Verified, @Hash, @Now, @Now);";
        var now = DateTime.UtcNow;
        await unitOfWork.ExecuteAsync(sql, new()
        {
            ["@Email"] = credentials.Item1,
            ["@Verified"] = emailVerified,
            ["@Hash"] = passwordHash,
            ["@Now"] = now,
        });

        await unitOfWork.CommitAsync();

        return credentials;
    }

    public (string, string) GenerateTestCredentials()
    {
        var email = GenerateTestEmail();
        var password = GenerateTestPassword();

        if (email == null)
        {
            throw new InvalidOperationException("email is null");
        }

        _testUsersToDispose.Add(email, password);

        return (email, password);
    }

    private string? GenerateTestEmail()
    {
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)); // 4 bytes = 8 hex chars
        
        // If the user defined a domain, send the test emails to random@domain.com
        if (_config.TestUsers?.Domain != null)
        {
            return $"{random}@{_config.TestUsers.Domain.Replace("@", "")}";
        }

        // If user defined an email (something like gmail or outlook), send the test emails to their_email+randomcode@gmail.com for example
        if (_config.TestUsers?.Email != null)
        {
            var email = _config.TestUsers.Email;
            var atIndex = email.IndexOf('@');
            if (atIndex == -1)
            {
                return null;
            }

            var local = email[..atIndex];
            var domain = email[(atIndex + 1)..];
            return $"{local}+{random}@{domain}";
        }
        
        return null;
    }

    private string GenerateTestPassword()
    {
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(7)); // 7 bytes = 14 hex chars
        return random;
    }
}
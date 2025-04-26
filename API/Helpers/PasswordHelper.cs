using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace DotNetBackendTemplate.Helpers;

public static class PasswordHelper
{
    // Versioning the hashes is useful incase we want to change the MemorySize, Iterations or DegreeOfParallelism in the future to increase
    // strength. We could then update a user's to the stronger new version on login without them having to do anything. This allows for easy
    // future proofing!
    private const string CurrentVersion = "argon2id-v1";

    private const int MemorySize = 65536; // 64MB
    private const int Iterations = 4;
    private const int DegreeOfParallelism = 8; // 8 threads

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        using var hasher = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var hash = hasher.GetBytes(32);

        return $"{CurrentVersion}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':', 3);
        if (parts.Length != 3)
            return false;

        var version = parts[0];
        var salt = Convert.FromBase64String(parts[1]);
        var originalHash = Convert.FromBase64String(parts[2]);

        if (version != CurrentVersion)
            return false;

        using var hasher = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var computedHash = hasher.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(computedHash, originalHash);
    }
}
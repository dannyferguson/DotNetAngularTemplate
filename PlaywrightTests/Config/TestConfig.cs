namespace PlaywrightTests.Config;

using System.Text.Json;

public class TestUsersConfig
{
    public string? Domain { get; set; }
    public string? Email { get; set; }
}

public class TestConfig
{
    public bool Headless { get; set; }
    public int SlowMo { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public TestUsersConfig? TestUsers { get; set; }

    public static TestConfig Load()
    {
        var json = File.ReadAllText("testsettings.json");
        var config = JsonSerializer.Deserialize<TestConfig>(json);

        if (config is null)
        {
            throw new InvalidOperationException("Failed to deserialize 'testsettings.json'.");
        }

        if (config.TestUsers is null)
        {
            throw new InvalidOperationException("'TestUsers' section is missing in testsettings.json.");
        }
        
        return config!;
    }
}

namespace PlaywrightTests.Config;

using System.Text.Json;

public class TestConfig
{
    public bool Headless { get; set; }
    public int SlowMo { get; set; }
    public string BaseUrl { get; set; } = string.Empty;

    public static TestConfig Load()
    {
        var json = File.ReadAllText("testsettings.json");
        return JsonSerializer.Deserialize<TestConfig>(json)!;
    }
}

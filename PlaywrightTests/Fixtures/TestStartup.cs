namespace PlaywrightTests.Fixtures;

public class TestStartup
{
    public static bool Initialized { get; private set; }

    static TestStartup()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new Exception($"Playwright exited with code {exitCode}");
        }

        Initialized = true;
    }
}

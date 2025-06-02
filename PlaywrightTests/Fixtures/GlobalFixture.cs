namespace PlaywrightTests.Fixtures;

public class GlobalFixture : IDisposable
{
    public GlobalFixture()
    {
        _ = PlaywrightInstallFixture.Initialized;
    }

    public void Dispose()
    {
        // Optional: global teardown
    }
}
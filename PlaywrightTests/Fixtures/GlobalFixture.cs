namespace PlaywrightTests.Fixtures;

public class GlobalFixture : IDisposable
{
    public GlobalFixture()
    {
        _ = TestStartup.Initialized;
    }

    public void Dispose()
    {
        // Optional: global teardown
    }
}
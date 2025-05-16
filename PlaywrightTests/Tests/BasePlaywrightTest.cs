using PlaywrightTests.Config;

namespace PlaywrightTests.Tests;

using Microsoft.Playwright;

public abstract class BasePlaywrightTest : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private readonly TestConfig _config = TestConfig.Load();
    
    protected IPage Page = null!;
    protected string BaseUrl = string.Empty;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        BaseUrl = _config.BaseUrl;

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _config.Headless,
            SlowMo = _config.SlowMo
        });

        _context = await _browser.NewContextAsync();
        Page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
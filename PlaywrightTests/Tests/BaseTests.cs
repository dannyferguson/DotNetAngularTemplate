namespace PlaywrightTests.Tests;

[Collection("Global Test Setup")]
public class BaseTests : BasePlaywrightTest
{
    [Fact]
    public async Task IndexLoads()
    {
        await Page.GotoAsync(BaseUrl);

        // Get page h2
        var h2 = await Page.Locator("h2").TextContentAsync();

        // Expect h2 to contain welcome wording
        Assert.Matches("Welcome", h2);
    }
}

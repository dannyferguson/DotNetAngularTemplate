namespace PlaywrightTests.Tests;

[Collection("Global Test Setup")]
public class BaseTests : BasePlaywrightTest
{
    [Fact]
    public async Task IndexLoads()
    {
        await Page.GotoAsync(BaseUrl);

        // Get page title
        var pageTitle = await Page.Locator("h2").TextContentAsync();

        // Expect title to contain welcome message
        Assert.Matches("Welcome", pageTitle);
    }
}

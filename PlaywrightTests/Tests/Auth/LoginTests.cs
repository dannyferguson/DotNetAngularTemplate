using PlaywrightTests.Fixtures;

namespace PlaywrightTests.Tests.Auth;

[Collection("Global Test Setup")]
public class LoginTests : BasePlaywrightTest
{
    private readonly DatabaseFixture _db;

    public LoginTests(DatabaseFixture db)
    {
        _db = db;
    }
    
    [Fact]
    public async Task LoginPageLoads()
    {
        await Page.GotoAsync($"{BaseUrl}/login");

        // Get page h2
        var h2 = await Page.Locator("h2").TextContentAsync();

        // Expect h2 to contain login wording
        Assert.Matches("Login", h2);
    }
    
    [Fact]
    public async Task LoginFormContainsExpectedFields()
    {
        await Page.GotoAsync($"{BaseUrl}/login");

        // Get form fields
        var emailField = await Page.QuerySelectorAsync("#email");
        var passwordField = await Page.QuerySelectorAsync("#password");
        var submitButton = await Page.QuerySelectorAsync("button[type='submit']");

        // Expect all fields to exist on the page
        Assert.NotNull(emailField);
        Assert.NotNull(passwordField);
        Assert.NotNull(submitButton);
    }
    
    [Fact]
    public async Task LoginFormErrorsWhenNoInfoEntered()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        
        // Attempt to submit form
        await Page.ClickAsync("button[type='submit']");
        
        // Assert we see 2 form field validation errors
        Assert.Equal(2, await Page.Locator(".text-input-error").CountAsync());
    }
    
    [Fact]
    public async Task LoginFormWorksWithVerifiedAccount()
    {
        var (email, password) = await _db.GenerateAccount(true); 

        await Page.GotoAsync($"{BaseUrl}/login");

        // Fill in login form
        var emailInput = Page.Locator("input#email");
        await emailInput.FillAsync(email);
        
        var passwordInput = Page.Locator("input#password");
        await passwordInput.FillAsync(password);
        
        await Page.ClickAsync("button[type='submit']");
        
        // Assert we see a success snackbar
        Assert.True(await Page.Locator("text=Login successful! Redirecting..").IsVisibleAsync());
        
        await Page.WaitForURLAsync($"{BaseUrl}/");

        // Assert we were redirected to homepage
        Assert.Equal($"{BaseUrl}/", Page.Url);
    }
    
    [Fact]
    public async Task LoginFormErrorsWithUnverifiedAccount()
    {
        var (email, password) = await _db.GenerateAccount(false); 

        await Page.GotoAsync($"{BaseUrl}/login");

        // Fill in login form
        var emailInput = Page.Locator("input#email");
        await emailInput.FillAsync(email);
        
        var passwordInput = Page.Locator("input#password");
        await passwordInput.FillAsync(password);
        
        await Page.ClickAsync("button[type='submit']");

        // Assert we see an error snackbar
        Assert.True(await Page.Locator("text=Email not verified. Please confirm your email before logging in.").IsVisibleAsync());
    }
    
    [Fact]
    public async Task LoginFormErrorsWithInvalidEmail()
    {
        await Page.GotoAsync($"{BaseUrl}/login");

        // Fill in login form
        var emailInput = Page.Locator("input#email");
        await emailInput.FillAsync("doesnt@exist.com");
        
        var passwordInput = Page.Locator("input#password");
        await passwordInput.FillAsync("supersecurepassword123");
        
        await Page.ClickAsync("button[type='submit']");

        // Assert we see an error snackbar
        Assert.True(await Page.Locator("text=Invalid credentials. Please try again.").IsVisibleAsync());
    }
    
    [Fact]
    public async Task LoginFormErrorsWithInvalidPassword()
    {
        var (email, password) = await _db.GenerateAccount(true); 
        
        await Page.GotoAsync($"{BaseUrl}/login");

        // Fill in login form
        var emailInput = Page.Locator("input#email");
        await emailInput.FillAsync(email);
        
        var passwordInput = Page.Locator("input#password");
        await passwordInput.FillAsync("thisiswrong123456");
        
        await Page.ClickAsync("button[type='submit']");

        // Assert we see an error snackbar
        Assert.True(await Page.Locator("text=Invalid credentials. Please try again.").IsVisibleAsync());
    }
}

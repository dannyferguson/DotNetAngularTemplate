using PlaywrightTests.Fixtures;

namespace PlaywrightTests.Tests.Auth;

[Collection("Global Test Setup")]
public class RegisterTests : BasePlaywrightTest
{
    private readonly DatabaseFixture _db;

    public RegisterTests(DatabaseFixture db)
    {
        _db = db;
    }
    
    [Fact]
    public async Task RegisterPageLoads()
    {
        await Page.GotoAsync($"{BaseUrl}/register");

        // Get page h2
        var h2 = await Page.Locator("h2").TextContentAsync();

        // Expect h2 to contain register wording
        Assert.Matches("Register", h2);
    }
    
    [Fact]
    public async Task RegisterFormContainsExpectedFields()
    {
        await Page.GotoAsync($"{BaseUrl}/register");

        // Get form fields
        var emailField = await Page.QuerySelectorAsync("#email");
        var passwordField = await Page.QuerySelectorAsync("#password");
        var passwordConfirmationField = await Page.QuerySelectorAsync("#passwordConfirmation");
        var submitButton = await Page.QuerySelectorAsync("button[type='submit']");

        // Expect all fields to exist on the page
        Assert.NotNull(emailField);
        Assert.NotNull(passwordField);
        Assert.NotNull(passwordConfirmationField);
        Assert.NotNull(submitButton);
    }
    
    [Fact]
    public async Task RegisterFormErrorsWhenNoInfoEntered()
    {
        await Page.GotoAsync($"{BaseUrl}/register");
        
        // Attempt to submit form
        await Page.ClickAsync("button[type='submit']");
        
        // Assert we see 3 form field validation errors
        Assert.Equal(3, await Page.Locator(".text-input-error").CountAsync());
    }
    
    [Fact]
    public async Task RegisterFormWorksWithValidInfo()
    {
        var (email, password) = _db.GenerateTestCredentials();
        
        await Page.GotoAsync($"{BaseUrl}/register");
        
        // Attempt to submit form
        var emailField = await Page.QuerySelectorAsync("input#email");
        var passwordField = await Page.QuerySelectorAsync("input#password");
        var passwordConfirmationField = await Page.QuerySelectorAsync("input#passwordConfirmation");
        
        Assert.NotNull(emailField);
        Assert.NotNull(passwordField);
        Assert.NotNull(passwordConfirmationField);

        await emailField.FillAsync(email);
        await passwordField.FillAsync(password);
        await passwordConfirmationField.FillAsync(password);
        
        await Page.ClickAsync("button[type='submit']");
        
        // Assert we see a success toast
        Assert.True(await Page.Locator("text=Registration successful. Please check your email to verify your account.").IsVisibleAsync());
    }
    
    [Fact]
    public async Task RegisterFormFakeWorksWhenRegisteringADuplicateEmail()
    {
        var (email, password) = await _db.GenerateAccount(false);
        
        await Page.GotoAsync($"{BaseUrl}/register");
        
        // Attempt to submit form
        var emailField = await Page.QuerySelectorAsync("input#email");
        var passwordField = await Page.QuerySelectorAsync("input#password");
        var passwordConfirmationField = await Page.QuerySelectorAsync("input#passwordConfirmation");
        
        Assert.NotNull(emailField);
        Assert.NotNull(passwordField);
        Assert.NotNull(passwordConfirmationField);

        await emailField.FillAsync(email);
        await passwordField.FillAsync(password);
        await passwordConfirmationField.FillAsync(password);
        
        await Page.ClickAsync("button[type='submit']");
        
        // Assert we see a success toast
        Assert.True(await Page.Locator("text=Registration successful. Please check your email to verify your account.").IsVisibleAsync());
    }
}

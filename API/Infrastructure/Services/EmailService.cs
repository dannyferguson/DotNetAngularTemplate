using Resend;

namespace DotNetAngularTemplate.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger, IConfiguration config, IResend resend)
{
    private readonly string _fromEmail = config["Custom:EmailFrom"]!;

    public async Task SendForgotPasswordEmail(string email, string code)
    {
        var subject = "Password Reset";
        var content = LoadTemplate("PasswordResetEmailTemplate.html")
            .Replace("{{RESET_LINK}}",
                $"{config["Custom:BaseUrl"]}/forgot-password-confirmation?code={code}&email={email}");
        await SendEmail(email, subject, content);
    }
    
    public async Task SendPasswordChangedEmail(string email)
    {
        var subject = "Password Changed";
        var content = LoadTemplate("PasswordChangedEmailTemplate.html");
        await SendEmail(email, subject, content);
    }

    public async Task<bool> SendRegistrationEmail(string email, string code)
    {
        var subject = "Welcome";
        var content = LoadTemplate("RegistrationEmailTemplate.html")
            .Replace("{{CONFIRMATION_LINK}}", $"{config["Custom:BaseUrl"]}/confirm-email?code={code}");
        return await SendEmail(email, subject, content);
    }

    private async Task<bool> SendEmail(string toEmail, string subject, string htmlBody)
    {
        var message = new EmailMessage();
        message.From = _fromEmail;
        message.To.Add( toEmail );
        message.Subject = subject;
        message.HtmlBody = htmlBody;

        try
        {
            var response = await resend.EmailSendAsync( message );

            if (response.Success)
            {
                return true;
            }

            logger.LogError(response.Exception, "Unable to send email to \"{email}\" with subject \"{subject}\"", toEmail, subject);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to send email to \"{email}\" with subject \"{subject}\"", toEmail, subject);
            return false;
        }
    }
    
    private string LoadTemplate(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", templateName);
        var html = File.ReadAllText(path);

        html = html.Replace("{{APP_NAME}}", config["Custom:AppName"])
            .Replace("{{SUPPORT_EMAIL}}", config["Custom:SupportEmail"])
            .Replace("{{BASE_URL}}", config["Custom:BaseUrl"]);
        
        return html;
    }
}
using DotNetAngularTemplate.Models;
using Resend;

namespace DotNetAngularTemplate.Services;

public class EmailService(ILogger<EmailService> logger, IConfiguration config, IResend resend)
{
    private string _fromEmail = config.GetSection("Emails:From").Value!;

    public async Task<Result> SendForgotPasswordEmail(string email)
    {
        var subject = "Password Reset";
        var content = """
                            This is a test
                            Crodie
                           """;
        return await SendEmail(email, subject, content);
    }

    private async Task<Result> SendEmail(string toEmail,  string subject, string htmlBody)
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
                return Result.Success();
            }

            logger.LogError(response.Exception, "Unable to send email to \"{email}\" with subject \"{subject}\"", toEmail, subject);
            return Result.Failure("Unable to send email");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to send email to \"{email}\" with subject \"{subject}\"", toEmail, subject);
            return Result.Failure("oops");
        }
    }
}
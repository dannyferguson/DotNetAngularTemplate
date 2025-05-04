namespace DotNetAngularTemplate.Middleware;

public class GetEmailFromRequestMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals("/api/v1/auth/forgot-password", StringComparison.OrdinalIgnoreCase)
            && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            try
            {
                var email = System.Text.Json.JsonDocument.Parse(body).RootElement
                    .GetProperty("email").GetString();

                if (!string.IsNullOrWhiteSpace(email))
                {
                    context.Items["ForgotPasswordEmail"] = email.ToLowerInvariant();
                }
            }
            catch
            {
                // Fallback to IP if email not provided
                context.Items["ForgotPasswordEmail"] = context.Connection.RemoteIpAddress?.ToString();
            }
        }

        await next(context);
    }
}
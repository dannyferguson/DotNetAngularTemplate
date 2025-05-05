# DotNetAngularTemplate

This is going to serve as an *opinionated* personal template for deploying basic ASP.NET + Angular SPA apps. Meaning .NET will handle all the backend stuff while Angular will handle the UI in SPA mode.

**Demo: https://dotnetangular.ondanny.com/**

Considerations:
* While its easy to add OAuth authentication, for my personal projects I prefer to keep it to the basic email/password combo. That way I don't have to worry about user counts/billing.

* Another preference when it comes to personal projects is that I do not care for an ORM. I know enough SQL to write the queries myself.

* I also prefer doing session/cookie based authentication versus JWT since users are more in control of their sessions. And scaling would really not be much of a problem (centralized cache like redis + more backend instances)

## Environment Variables

There are a few required environment variables to run this:
### DotNet
* `ASPNETCORE_URLS` Use this to set the url/port the application will run on, for example `http://+:5000` will run on 0.0.0.0:5000 which my docker example will expose on 0.0.0.0:8080

* This one might be obvious but `ASPNETCORE_ENVIRONMENT` to either `Production` or `Development` respectively.

### Application Specific
* `ConnectionStrings__Default` which is the MySQL connection string (used for all the data except sessions), typically in the following format: `Server=127.0.0.1;Port=3306;Database=dotnetangulardb;Uid=root;Pwd=supersecure123;`

* `ConnectionStrings__Redis` which is as you've guessed the Redis connection string (used for session storage and email rate limiting). Format: `127.0.0.1:6379,password=supersecuremuchlonger123`

* `Emails__ResendApiKey` [Resend](https://resend.com/) API key. This is the service I choose for my SMTP needs. It's pricier than using AWS SES but comes with many quality of life improvements and an easy to set up free tier.

* `Emails__From` Which email address to send all your automated emails from, typically something like `noreply@yourdomain.com`.

## Docker

For the Docker enjoyers out there, I've provided a Dockerfile for the project which should be easily customizable. 

I've also provided a docker-compose.yml that gives you a stack running this application, redis and mysql. That way you can run and test the whole thing easily.

## Rate Limits

I added some basic rate limiting. 100 requests per minute globally, or 10 per minute for the auth controller. You can configure these in [Program.cs](/API/Program.cs)

## CSP (Content Security Policy)
I setup a pretty strict CSP that you will need to modify if you serve anything from another domain/subdomain, see [CspNonceMiddleware.cs](API/Middleware/CspNonceMiddleware.cs). You can see how it gets injected in the responses by looking at [IndexHtmlNonceInjectionMiddleware.cs](API/Middleware/IndexHtmlNonceInjectionMiddleware.cs)

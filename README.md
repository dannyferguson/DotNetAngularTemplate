# DotNetAngularTemplate

This is going to serve as an *opinionated* personal template for deploying basic ASP.NET + Angular SPA apps. Meaning .NET will handle all the backend stuff while Angular will handle the UI in SPA mode.

Considerations:
* While its easy to add OAuth authentication, for my personal projects I prefer to keep it to the basic email/password combo. That way I don't have to worry about user counts/billing.

* Another preference when it comes to personal projects is that I do not care for an ORM. I know enough SQL to write the queries myself.

* I also prefer doing session/cookie based authentication versus JWT since users are more in control of their sessions. And scaling would really not be much of a problem (centralized cache like redis + more backend instances)
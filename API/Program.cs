using DotNetBackendTemplate.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add in-mem cache + sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.IdleTimeout = TimeSpan.FromDays(7); 
});

// Add MySQL support/connection
var mysqlConnectionString = builder.Configuration.GetConnectionString("Default");
if (mysqlConnectionString == null)
{
    Console.WriteLine("Missing environment variable ConectionStrings__Default. Please set it before running the application!");
    Environment.Exit(1);
}
builder.Services.AddSingleton<DatabaseService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
    return new DatabaseService(logger, mysqlConnectionString);
});
builder.Services.AddSingleton<AuthService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve Angular from wwwroot folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "browser")),
    RequestPath = ""
}); 

app.UseSession();
app.MapControllers();

// Map all requests not at /api to Angular
app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder => {
        appBuilder.UseRouting();
        appBuilder.UseEndpoints(endpoints => {
            endpoints.MapFallbackToFile("/browser/index.html");
        });
    }
);

app.Run();
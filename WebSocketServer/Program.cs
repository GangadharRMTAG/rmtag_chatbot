
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using WebSocketServer.Models;
using WebSocketServer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebSocketServer;
using Microsoft.Extensions.FileProviders;



var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var environment = builder.Environment;
var contentRoot = environment.ContentRootPath;
builder.WebHost.UseContentRoot(contentRoot);


builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddHttpClient();

builder.Services.AddScoped<WebSocketHandler>();

string connectionString = null;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogInformation("Current Environment: {Environment}", builder.Environment.EnvironmentName);

    var databaseUrl = builder.Configuration["DATABASE_URL"] ?? Environment.GetEnvironmentVariable("DATABASE_URL");
    logger.LogInformation("Retrieved DATABASE_URL: {DatabaseUrl}", databaseUrl);

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];
            var host = uri.Host;
            var port = uri.Port;
            var dbname = uri.AbsolutePath.TrimStart('/');

            connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={dbname};";
            logger.LogInformation("Using the connection string: {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing DATABASE_URL environment variable.");
            throw new Exception("Error parsing DATABASE_URL environment variable.", ex);
        }
    }
    else
    {
        logger.LogError("DATABASE_URL environment variable is missing.");
        throw new Exception("DATABASE_URL environment variable is missing.");
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError("Database connection string is not set. Please check your configuration.");
    throw new Exception("Database connection string is not set.");
}

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();


var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (Directory.Exists(wwwrootPath))
{
    var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("The 'wwwroot' directory exists.");

    var files = Directory.GetFiles(wwwrootPath);
    if (files.Length > 0)
    {
        logger.LogInformation("Files in 'wwwroot':");
        foreach (var file in files)
        {
            logger.LogInformation(file); // Log each file in wwwroot
        }
    }
    else
    {
        logger.LogInformation("No files found in 'wwwroot'.");
    }
}
else
{
    var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError("The 'wwwroot' directory does not exist.");
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.Migrate();
        var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseCors("AllowAll");
// app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    // OnPrepareResponse = ctx =>
    // {
    //     Console.WriteLine($"Serving static file: {ctx.File.Name}");
    // }
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/wwwroot"
});



app.UseRouting();
app.UseWebSockets();
app.UseAuthorization();
app.MapControllers();


app.Map("/ws", async (HttpContext context) =>
{
    var scope = context.RequestServices.CreateScope();
    var webSocketHandler = scope.ServiceProvider.GetRequiredService<WebSocketHandler>();

    if (context.WebSockets.IsWebSocketRequest)
    {
        var username = context.Request.Query["username"].ToString();
        var roomname = context.Request.Query["roomname"].ToString();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(roomname))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing username or roomname in query parameters.");
            return;
        }

        var user = new User { Username = username, roomname = roomname };

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await webSocketHandler.Handle(webSocket, user);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapFallbackToFile("index.html");

// app.Run(url: "http://*:8080");

var port1 = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://*:{port1}");

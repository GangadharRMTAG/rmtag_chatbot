
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


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WebSocketHandler>();

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

// Check if DATABASE_URL is accessible through Configuration or directly via Environment
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

            // var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
            // var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Using the connection string: {ConnectionString}", connectionString);
        }
        catch (Exception ex)
        {
            // var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
            // var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "Error parsing DATABASE_URL environment variable.");
            throw new Exception("Error parsing DATABASE_URL environment variable.", ex);
        }
    }
    else
    {
        // var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        // var logger = loggerFactory.CreateLogger<Program>();
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
app.UseStaticFiles();
app.UseRouting();
app.UseWebSockets();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapWebSocketManager("/ws", app.Services.GetService<WebSocketHandler>());

app.Run(url: "http://*:8080");




public class WebSocketHandler
{
    private readonly Dictionary<string, List<User>> _roomUsers = new();
    private readonly Dictionary<string, WebSocket> _sockets = new();

    public async Task Handle(WebSocket webSocket, User user)
    {
        Console.WriteLine("New WebSocket connection established.");
        var buffer = new byte[1024 * 4];

        AddSocketToRoom(user.roomname, user, webSocket);

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                var parts = message.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    var roomName = parts[0].Trim();
                    var msg = parts[1].Trim();

                    await SendMessageToRoom(roomName, msg);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
        finally
        {
            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            RemoveSocketFromRoom(user.roomname, user.Username);
        }
    }

    private void AddSocketToRoom(string roomName, User user, WebSocket socket)
    {
        if (!_roomUsers.ContainsKey(roomName))
        {
            _roomUsers[roomName] = new List<User>();
        }

        _roomUsers[roomName].Add(user);
        _sockets[user.Username] = socket; 

        NotifyUsersInRoom(roomName);
    }

    private void RemoveSocketFromRoom(string roomName, string username)
    {
        if (_roomUsers.ContainsKey(roomName))
        {
            _roomUsers[roomName].RemoveAll(u => u.Username == username);

            NotifyUsersInRoom(roomName);
        }
    }

    private async void NotifyUsersInRoom(string roomName)
    {
        if (_roomUsers.ContainsKey(roomName))
        {
            var connectedUsers = _roomUsers[roomName].Select(u => u.Username).ToList();
            Console.WriteLine($"Connected users in {roomName}: {string.Join(", ", connectedUsers)}");
            string userListMessage = $"Connected users in {roomName}: {string.Join(", ", connectedUsers)}";

            await SendMessageToRoom(roomName, userListMessage);
        }
    }

    private async Task SendMessageToRoom(string roomName, string message)
    {
        if (_roomUsers.ContainsKey(roomName))
        {
            foreach (var user in _roomUsers[roomName])
            {
                if (_sockets.TryGetValue(user.Username, out var socket) && socket.State == WebSocketState.Open)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
public static class WebSocketExtensions
{
    public static void MapWebSocketManager(this IEndpointRouteBuilder endpoints, string path, WebSocketHandler handler)
    {
        endpoints.MapGet(path, async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                
                var username = context.Request.Query["username"];
                var roomname = context.Request.Query["roomname"];

                var user = new User { Username = username, roomname = roomname };

                await handler.Handle(webSocket, user);
            }
            else
            {
                context.Response.StatusCode = 400; 
            }
        });
    }
}




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

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WebSocketHandler>(); 

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//                        ?? builder.Configuration["DATABASE_URL"];

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["DATABASE_URL"];


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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseWebSockets(); 
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
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



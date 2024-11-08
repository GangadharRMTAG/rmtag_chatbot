using System;
using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebSocketServer.Data;
using WebSocketServer.Models;

namespace WebSocketServer;
public class WebSocketHandler
{
    private static readonly Dictionary<string, List<User>> _roomUsers = new();
    private static readonly Dictionary<string, WebSocket> _sockets = new();
    private readonly DataContext _dbContext;

    public WebSocketHandler(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(WebSocket webSocket, User user)
    {
        Console.WriteLine("New WebSocket connection established.");
        var buffer = new byte[1024 * 4];

        AddSocketToRoom(user.roomname, user, webSocket);

        await SendMessageHistory(user.roomname, user);

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
                var msg = message;

                await SaveMessageToDatabase(user.Username, user.roomname, msg);

                await SendMessageToRoom(user.roomname, msg);

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

    private async Task SaveMessageToDatabase(string username, string roomname, string messageText)
    {
        var message = new Message
        {
            Username = username,
            Roomname = roomname,
            MessageText = messageText,
            Timestamp = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SendMessageHistory(string roomName, User user)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.Roomname == roomName)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        foreach (var message in messages)
        {
            string messageToSend = $"{message.Username}: {message.MessageText}";
            if (_sockets.TryGetValue(user.Username, out var socket) && socket.State == WebSocketState.Open)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(messageToSend);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
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

    private void AddSocketToRoom(string roomName, User user, WebSocket socket)
    {
        lock (_roomUsers)
        {
            if (!_roomUsers.ContainsKey(roomName))
            {
                _roomUsers[roomName] = new List<User>();
            }

            if (!_roomUsers[roomName].Any(u => u.Username == user.Username))
            {
                _roomUsers[roomName].Add(user);
                lock (_sockets)
                {
                    _sockets[user.Username] = socket;
                }

                NotifyUsersInRoom(roomName);
            }
        }
    }

    private void RemoveSocketFromRoom(string roomName, string username)
    {
        lock (_roomUsers)
        {
            if (_roomUsers.ContainsKey(roomName))
            {
                _roomUsers[roomName].RemoveAll(u => u.Username == username);

                lock (_sockets)
                {
                    _sockets.Remove(username);
                }

                NotifyUsersInRoom(roomName);
            }
        }
    }

    private async void NotifyUsersInRoom(string roomName)
    {
        if (_roomUsers.ContainsKey(roomName))
        {
            var connectedUsers = _roomUsers[roomName].Select(u => u.Username).ToList();
            Console.WriteLine($"Connected users in {roomName}: {string.Join(", ", connectedUsers)}");
            var userListMessage = $"Connected users in {roomName}: {string.Join(", ", connectedUsers)}";

            await SendMessageToRoom(roomName, userListMessage);
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


using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketServer.Models;
using WebSocketServer.Data;
using Microsoft.EntityFrameworkCore;

namespace WebSocketServer.Services;


public interface IChatService
{
    Task<List<ChatMessage>> GetMessagesAsync();
    Task AddMessageAsync(ChatMessage message);
}

public class ChatService : IChatService
{
    private readonly DataContext _context;

    public ChatService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        return await _context.ChatMessages.ToListAsync();
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        message.Timestamp = DateTime.UtcNow; 
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
    }
}

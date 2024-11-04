using System;
using Microsoft.EntityFrameworkCore;
using WebSocketServer.Models;

namespace WebSocketServer.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }

}

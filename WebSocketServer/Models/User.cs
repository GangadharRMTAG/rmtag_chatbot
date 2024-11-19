
namespace WebSocketServer.Models
{    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? roomname { get; set; }
        public DateTime ConnectionTime { get; set; } = DateTime.UtcNow;
        public string? Email { get; set; }
        public string? Password { get; set; }   
    }
}



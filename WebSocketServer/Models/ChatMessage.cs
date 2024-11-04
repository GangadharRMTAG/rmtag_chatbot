using System;

namespace WebSocketServer.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

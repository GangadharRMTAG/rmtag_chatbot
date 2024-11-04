using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSocketServer.Models;

[Table("message")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Roomname { get; set; }

        [Required]
        public string? MessageText { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }




    

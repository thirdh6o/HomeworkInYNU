using System;

namespace SIPClient.Models
{
    public class Message
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsFromMe { get; set; }
        public bool IsRead { get; set; }
    }
}
using System;

namespace SIPClient.Models
{
    public class Contact
    {
        public string DisplayName { get; set; } = string.Empty;
        public string SipUri { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
using System;

namespace HackathonHub.Client.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
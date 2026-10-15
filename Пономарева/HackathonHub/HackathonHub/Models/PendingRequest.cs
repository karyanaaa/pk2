using System;

namespace HackathonHub.Client.Models
{
    public class PendingRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
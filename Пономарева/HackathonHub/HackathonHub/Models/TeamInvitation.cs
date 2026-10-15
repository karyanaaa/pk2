using System;

namespace HackathonHub.Client.Models
{
    public class TeamInvitation
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string HackathonTitle { get; set; }
        public string FromUserName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
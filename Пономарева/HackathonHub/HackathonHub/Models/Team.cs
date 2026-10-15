using System;
using System.Collections.Generic;

namespace HackathonHub.Client.Models
{
    public class Team
    {
        public int Id { get; set; }
        public int HackathonId { get; set; }
        public int CaptainId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string HackathonTitle { get; set; }
        public string CaptainName { get; set; }
        public List<User> Members { get; set; } = new List<User>();
        public Project Project { get; set; }

        public string SubmissionStatus { get; set; }
        public int? HoursLeft { get; set; }

        public string SubmissionIcon => SubmissionStatus switch
        {
            "submitted" => "✅",
            "pending" => "⏳",
            "missed" => "❌",
            _ => "—"
        };

        public string SubmissionText => SubmissionStatus switch
        {
            "submitted" => "Сдан",
            "pending" when HoursLeft.HasValue => $"Осталось {HoursLeft} ч",
            "pending" => "Не сдан",
            "missed" => "Дедлайн прошёл",
            _ => "—"
        };
    }
}
using System.Collections.Generic;

namespace HackathonHub.Client.Models
{
    public class TeamOverview
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int HackathonId { get; set; }
        public string HackathonTitle { get; set; }
        public string CaptainName { get; set; }
        public int MembersCount { get; set; }
        public List<TeamMemberBrief> Members { get; set; } = new List<TeamMemberBrief>();

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
            "submitted" => "Проект сдан",
            "pending" when HoursLeft.HasValue => $"Не сдан (осталось {HoursLeft} ч)",
            "pending" => "Не сдан",
            "missed" => "Дедлайн прошёл",
            _ => "—"
        };
    }

    public class TeamMemberBrief
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
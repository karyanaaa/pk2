using System;
using System.Collections.Generic;

namespace HackathonHub.Client.Models
{
    public class PublicProfile
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Skills { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Participation> Participations { get; set; } = new List<Participation>();

        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);
        public System.Windows.Visibility AvatarVisibility =>
            HasAvatar ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility InitialVisibility =>
            HasAvatar ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        public string Initial =>
            !string.IsNullOrEmpty(FullName) ? FullName[0].ToString().ToUpper() : "?";

        public string RoleText => Role switch
        {
            "admin" => "Администратор",
            "organizer" => "Организатор",
            "participant" => "Участник",
            "jury" => "Жюри",
            _ => Role
        };
    }

    public class Participation
    {
        public int? HackathonId { get; set; }
        public string HackathonTitle { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }

        public string StatusText => Status switch
        {
            "open" => "открыт",
            "ongoing" => "идёт",
            "finished" => "завершён",
            "closed" => "закрыт",
            _ => Status
        };
    }
}
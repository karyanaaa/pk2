using System;

namespace HackathonHub.Client.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string Skills { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
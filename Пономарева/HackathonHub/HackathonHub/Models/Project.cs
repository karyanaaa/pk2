using System;

namespace HackathonHub.Client.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int? HackathonId { get; set; }
        public string HackathonTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RepositoryUrl { get; set; }
        public string FilePath { get; set; }
        public string PresentationPath { get; set; }
        public DateTime UploadedAt { get; set; }
        public string TeamName { get; set; }
    }
}
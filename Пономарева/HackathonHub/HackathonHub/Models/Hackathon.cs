using System;

namespace HackathonHub.Client.Models
{
    public class Hackathon
    {
        public int Id { get; set; }
        public int OrganizerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public string Status { get; set; }
        public string Rules { get; set; }
        public string Prizes { get; set; }
        public string TaskDescription { get; set; }
        public int MaxTeams { get; set; }
        public string OrganizerName { get; set; }
    }
}
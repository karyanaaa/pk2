using System;

namespace HackathonHub.Client.Models
{
    public class Score
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int JuryId { get; set; }
        public int CriterionId { get; set; }
        public int Value { get; set; }
        public string Comment { get; set; }
        public DateTime ScoredAt { get; set; }
        public string JuryName { get; set; }
        public string CriterionName { get; set; }
        public string ProjectTitle { get; set; }
    }
}
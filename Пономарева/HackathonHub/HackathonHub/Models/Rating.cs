namespace HackathonHub.Client.Models
{
    public class Rating
    {
        public int Position { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string ProjectTitle { get; set; }
        public decimal TotalScore { get; set; }
        public int JuryCount { get; set; }
    }
}
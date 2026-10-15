namespace HackathonHub.Client.Models
{
    public class Criterion
    {
        public int Id { get; set; }
        public int HackathonId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxScore { get; set; }
        public decimal Weight { get; set; }
    }
}
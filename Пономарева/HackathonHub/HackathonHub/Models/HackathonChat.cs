using System.Collections.Generic;

namespace HackathonHub.Client.Models
{
    public class HackathonChat
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<string> TeamNames { get; set; } = new List<string>();

        public string TeamNamesText => TeamNames != null && TeamNames.Count > 0
            ? "Команды: " + string.Join(", ", TeamNames)
            : "Нет команды";
    }
}
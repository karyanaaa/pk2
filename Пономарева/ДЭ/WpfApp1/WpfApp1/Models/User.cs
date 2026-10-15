namespace WpfApp1.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }

        public string FullName =>
            $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}
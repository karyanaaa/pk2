using System.Data.Entity;
using System.Linq;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public static class AuthService
    {
        public static User Authenticate(string login)
        {
            using (var ctx = new ChudoObuvEntities())
            {
                var u = ctx.Users
                           .Include(x => x.Roles)
                           .FirstOrDefault(x => x.Login == login);

                if (u == null) return null;

                return new User
                {
                    UserId = u.UserId,
                    LastName = u.LastName,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    Login = u.Login,
                    Role = u.Roles != null ? u.Roles.RoleName : ""
                };
            }
        }
    }
}
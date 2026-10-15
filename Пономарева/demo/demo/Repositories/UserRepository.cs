using demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo.Repositories
{
    public static class UserRepository
    {
        // Статический список, выступающий в роли таблицы "Пользователи"
        public static List<User> Users { get; private set; } = new List<User>();

        // Статический конструктор для создания тестовых пользователей при запуске приложения
        static UserRepository()
        {
            Users.Add(new User { Login = "admin", Password = "123", Role = "Администратор", IsBlocked = false, FailedAttempts = 0 });
            Users.Add(new User { Login = "user", Password = "123", Role = "Пользователь", IsBlocked = false, FailedAttempts = 0 });
        }

        /// <summary>
        /// Поиск пользователя по логину.
        /// </summary>
        public static User GetUserByLogin(string login)
        {
            return Users.FirstOrDefault(u => u.Login.Equals(login, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Добавление нового пользователя. Возвращает false, если логин уже занят.
        /// </summary>
        public static bool AddUser(User newUser)
        {
            if (GetUserByLogin(newUser.Login) != null) return false;
            Users.Add(newUser);
            return true;
        }

        /// <summary>
        /// Блокировка пользователя.
        /// </summary>
        public static void BlockUser(string login)
        {
            var user = GetUserByLogin(login);
            if (user != null) user.IsBlocked = true;
        }

        /// <summary>
        /// Разблокировка пользователя и сброс счетчика неудачных попыток.
        /// </summary>
        public static void UnblockUser(string login)
        {
            var user = GetUserByLogin(login);
            if (user != null)
            {
                user.IsBlocked = false;
                user.FailedAttempts = 0;
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusParkManagementSystem
{
    public static class CurrentUser
    {
        public static User User { get; set; }
        public static List<UserPermission> Permissions { get; set; } = new List<UserPermission>();

        public static bool IsAuthenticated => User != null && User.IsActive;

        public static async Task<bool> HasPermissionAsync(string menuCode, string accessType)
        {
            if (!IsAuthenticated) return false;

            // Если пользователь администратор, то у него все права
            if (User.Role == "Администратор") return true;

            // Проверяем права
            if (Permissions == null) return false;

            foreach (var permission in Permissions)
            {
                if (permission.MenuItemCode == menuCode)
                {
                    switch (accessType.ToLower())
                    {
                        case "read": return permission.CanRead;
                        case "write": return permission.CanWrite;
                        case "edit": return permission.CanEdit;
                        case "delete": return permission.CanDelete;
                    }
                }
            }

            return false;
        }

        public static void Logout()
        {
            User = null;
            Permissions = null;
        }

        public static async Task<bool> ValidatePasswordAsync(string password)
        {
            if (!IsAuthenticated || User == null) return false;

            // Проверяем пароль с использованием репозитория пользователей
            var userRepository = new Repositories.UserRepository();
            return await userRepository.ValidatePasswordAsync(User.Username, password);
        }

        public static async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (!IsAuthenticated || User == null) return false;

            // Меняем пароль с использованием репозитория пользователей
            var userRepository = new Repositories.UserRepository();
            return await userRepository.ChangePasswordAsync(User.Id, currentPassword, newPassword);
        }
    }
}
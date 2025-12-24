using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;


namespace BusParkManagementSystem
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public int? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool MustChangePassword { get; set; }
    }
}

// Модель для прав RWED
public class UserPermission
{
    public int UserId { get; set; }
    public string MenuItemId { get; set; } // Или int, в зависимости от структуры меню
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

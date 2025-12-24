// Repositories/IUserRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> CheckUsernameExistsAsync(string username);
        Task<List<UserPermission>> GetUserPermissionsAsync(int userId);
        Task<bool> HasPermissionAsync(int userId, string menuCode, string accessType);
    }
}
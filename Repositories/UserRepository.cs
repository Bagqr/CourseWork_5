using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BusParkManagementSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM system_users WHERE username = @username", connection);
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Role = reader.GetString(3),
                            EmployeeId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6),
                            LastLogin = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            MustChangePassword = reader.GetBoolean(8)
                        };
                    }
                }
                return null;
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM system_users WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Role = reader.GetString(3),
                            EmployeeId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6),
                            LastLogin = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            MustChangePassword = reader.GetBoolean(8)
                        };
                    }
                }
                return null;
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM system_users ORDER BY username", connection);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Role = reader.GetString(3),
                            EmployeeId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6),
                            LastLogin = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            MustChangePassword = reader.GetBoolean(8)
                        });
                    }
                }
            }
            return users;
        }

        public async Task<int> CreateAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"INSERT INTO system_users 
            (username, password_hash, role, employee_id, is_active, must_change_password) 
            VALUES (@username, @passwordHash, @role, @employeeId, @isActive, @mustChangePassword);
            SELECT last_insert_rowid();", connection);

                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@passwordHash", HashPassword(user.PasswordHash ?? ""));
                cmd.Parameters.AddWithValue("@role", user.Role);
                cmd.Parameters.AddWithValue("@employeeId", user.EmployeeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@isActive", user.IsActive);
                cmd.Parameters.AddWithValue("@mustChangePassword", user.MustChangePassword);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE system_users 
                    SET username = @username,
                        role = @role,
                        employee_id = @employeeId,
                        is_active = @isActive,
                        last_login = @lastLogin,
                        must_change_password = @mustChangePassword
                    WHERE id = @id", connection);

                cmd.Parameters.AddWithValue("@id", user.Id);
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@role", user.Role);
                cmd.Parameters.AddWithValue("@employeeId", user.EmployeeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@isActive", user.IsActive);
                cmd.Parameters.AddWithValue("@lastLogin", user.LastLogin ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@mustChangePassword", user.MustChangePassword);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("DELETE FROM system_users WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            if (!VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            var newHash = HashPassword(newPassword);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE system_users 
                    SET password_hash = @passwordHash,
                        must_change_password = 0
                    WHERE id = @userId", connection);

                cmd.Parameters.AddWithValue("@passwordHash", newHash);
                cmd.Parameters.AddWithValue("@userId", userId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var newHash = HashPassword(newPassword);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE system_users 
                    SET password_hash = @passwordHash,
                        must_change_password = 1
                    WHERE id = @userId", connection);

                cmd.Parameters.AddWithValue("@passwordHash", newHash);
                cmd.Parameters.AddWithValue("@userId", userId);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "UPDATE system_users SET last_login = datetime('now') WHERE id = @userId",
                    connection);
                cmd.Parameters.AddWithValue("@userId", userId);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "SELECT COUNT(*) FROM system_users WHERE username = @username",
                    connection);
                cmd.Parameters.AddWithValue("@username", username);
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<List<UserPermission>> GetUserPermissionsAsync(int userId)
        {
            var permissions = new List<UserPermission>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"SELECT up.*, mi.code as MenuItemCode, mi.name as MenuItemName
                    FROM user_permissions up
                    JOIN menu_items mi ON up.menu_item_id = mi.id
                    WHERE up.user_id = @userId", connection);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        permissions.Add(new UserPermission
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            MenuItemId = reader.GetInt32(2),
                            CanRead = reader.GetBoolean(3),
                            CanWrite = reader.GetBoolean(4),
                            CanEdit = reader.GetBoolean(5),
                            CanDelete = reader.GetBoolean(6),
                            MenuItemCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                            MenuItemName = reader.IsDBNull(8) ? null : reader.GetString(8)
                        });
                    }
                }
            }
            return permissions;
        }

        public async Task<bool> HasPermissionAsync(int userId, string menuCode, string accessType)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var sql = @"
                    SELECT COUNT(*) 
                    FROM user_permissions up
                    JOIN menu_items mi ON up.menu_item_id = mi.id
                    WHERE up.user_id = @userId 
                    AND mi.code = @menuCode";

                // Добавляем проверку конкретного права
                switch (accessType.ToLower())
                {
                    case "read": sql += " AND up.can_read = 1"; break;
                    case "write": sql += " AND up.can_write = 1"; break;
                    case "edit": sql += " AND up.can_edit = 1"; break;
                    case "delete": sql += " AND up.can_delete = 1"; break;
                }

                var cmd = new SQLiteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@menuCode", menuCode);

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        // Статические методы для хэширования
        public static string HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = md5.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
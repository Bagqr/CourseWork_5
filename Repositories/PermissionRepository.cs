using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Linq;

namespace BusParkManagementSystem.Repositories
{
    public interface IPermissionRepository
    {
        Task<List<UserPermission>> GetPermissionsForUserAsync(int userId);
        Task<List<MenuItem>> GetMenuItemsAsync();
        Task<bool> UpdatePermissionsAsync(int userId, List<UserPermission> permissions);
        Task<bool> EnsureTableExists();
    }

    public class PermissionRepository : IPermissionRepository
    {
        private readonly string _connectionString;

        public PermissionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> EnsureTableExists()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Проверяем существование таблицы
                    var checkTableSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='user_permissions'";
                    using (var cmd = new SQLiteCommand(checkTableSql, connection))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserPermission>> GetPermissionsForUserAsync(int userId)
        {
            var permissions = new List<UserPermission>();

            // Сначала проверяем существование таблицы
            if (!await EnsureTableExists())
            {
                Console.WriteLine("Таблица user_permissions не существует");
                return permissions; // Возвращаем пустой список
            }

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var cmd = new SQLiteCommand(
                        @"SELECT * FROM user_permissions WHERE user_id = @userId",
                        connection);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            permissions.Add(new UserPermission
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                MenuItemCode = reader.GetString(2),
                                CanRead = reader.GetBoolean(3),
                                CanWrite = reader.GetBoolean(4),
                                CanEdit = reader.GetBoolean(5),
                                CanDelete = reader.GetBoolean(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении прав пользователя: {ex.Message}");
            }

            return permissions;
        }

        public async Task<List<MenuItem>> GetMenuItemsAsync()
        {
            // Определяем все возможные модули системы
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Code = "buses", Name = "Автобусы" },
                new MenuItem { Code = "routes", Name = "Маршруты" },
                new MenuItem { Code = "trips", Name = "Рейсы" },
                new MenuItem { Code = "employees", Name = "Сотрудники" },
                new MenuItem { Code = "reports", Name = "Отчеты" },
                new MenuItem { Code = "queries", Name = "Запросы" },
                new MenuItem { Code = "lookups", Name = "Справочники" },
                new MenuItem { Code = "users", Name = "Пользователи" },
                new MenuItem { Code = "permissions", Name = "Права доступа" }
            };

            return await Task.FromResult(menuItems);
        }

        public async Task<bool> UpdatePermissionsAsync(int userId, List<UserPermission> permissions)
        {
            // Проверяем существование таблицы
            if (!await EnsureTableExists())
            {
                Console.WriteLine("Невозможно обновить права: таблица не существует");
                return false;
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Удаляем старые права
                        var deleteCmd = new SQLiteCommand(
                            "DELETE FROM user_permissions WHERE user_id = @userId",
                            connection, transaction);
                        deleteCmd.Parameters.AddWithValue("@userId", userId);
                        await deleteCmd.ExecuteNonQueryAsync();

                        // Добавляем новые права
                        foreach (var permission in permissions)
                        {
                            var insertCmd = new SQLiteCommand(
                                @"INSERT INTO user_permissions 
                                (user_id, menu_item_code, can_read, can_write, can_edit, can_delete)
                                VALUES (@userId, @code, @read, @write, @edit, @delete)",
                                connection, transaction);

                            insertCmd.Parameters.AddWithValue("@userId", userId);
                            insertCmd.Parameters.AddWithValue("@code", permission.MenuItemCode);
                            insertCmd.Parameters.AddWithValue("@read", permission.CanRead);
                            insertCmd.Parameters.AddWithValue("@write", permission.CanWrite);
                            insertCmd.Parameters.AddWithValue("@edit", permission.CanEdit);
                            insertCmd.Parameters.AddWithValue("@delete", permission.CanDelete);

                            await insertCmd.ExecuteNonQueryAsync();
                        }

                        // Используем синхронный Commit (без Async)
                        transaction.Commit();
                        Console.WriteLine($"Права для пользователя {userId} успешно обновлены");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обновлении прав: {ex.Message}");
                        // Используем синхронный Rollback (без Async)
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }

    public class MenuItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
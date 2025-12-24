using BusParkManagementSystem.Repositories;
using System;
using System.Data.SQLite;

namespace BusParkManagementSystem.Data
{
    public partial class DatabaseMigrator
    {
        private string _connectionString;

        public DatabaseMigrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void MigrateAuthenticationTables()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Создание всех таблиц системы аутентификации
                    CreateSystemUsersTable(connection);
                    CreateUserPermissionsTable(connection);

                    // Заполняем базового администратора
                    SeedDefaultAdmin(connection);

                    Console.WriteLine("Таблицы аутентификации успешно созданы");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при миграции таблиц аутентификации: {ex.Message}");
                throw;
            }
        }

        private void CreateSystemUsersTable(SQLiteConnection connection)
        {
            ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS system_users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password_hash TEXT NOT NULL,
                    role TEXT NOT NULL,
                    employee_id INTEGER,
                    is_active BOOLEAN DEFAULT 1,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    last_login DATETIME,
                    must_change_password BOOLEAN DEFAULT 0
                )");
        }

        private void CreateUserPermissionsTable(SQLiteConnection connection)
        {
            ExecuteSql(connection, @"
                CREATE TABLE IF NOT EXISTS user_permissions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    menu_item_code TEXT NOT NULL,
                    can_read BOOLEAN DEFAULT 0,
                    can_write BOOLEAN DEFAULT 0,
                    can_edit BOOLEAN DEFAULT 0,
                    can_delete BOOLEAN DEFAULT 0,
                    UNIQUE(user_id, menu_item_code)
                )");
        }

        private void SeedDefaultAdmin(SQLiteConnection connection)
        {
            // Проверяем, существует ли уже администратор
            var checkSql = "SELECT COUNT(*) FROM system_users WHERE username = 'admin'";
            using (var cmd = new SQLiteCommand(checkSql, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    // Хэшируем пароль "admin" с помощью UserRepository
                    var passwordHash = UserRepository.HashPassword("admin");

                    ExecuteSql(connection, @"
                        INSERT INTO system_users (username, password_hash, role, is_active, must_change_password)
                        VALUES ('admin', @hash, 'Администратор', 1, 0)",
                        new SQLiteParameter("@hash", passwordHash));

                    // Создаем права для администратора
                    CreateAdminPermissions(connection, 1); // ID 1 для admin

                    Console.WriteLine("Создан пользователь admin с паролем 'admin' и полными правами");
                }
            }
        }

        private void CreateAdminPermissions(SQLiteConnection connection, int adminId)
        {
            // Все возможные модули системы
            var menuItems = new[]
            {
                new { Code = "buses", Name = "Автобусы" },
                new { Code = "routes", Name = "Маршруты" },
                new { Code = "trips", Name = "Рейсы" },
                new { Code = "employees", Name = "Сотрудники" },
                new { Code = "reports", Name = "Отчеты" },
                new { Code = "queries", Name = "Запросы" },
                new { Code = "lookups", Name = "Справочники" },
                new { Code = "users", Name = "Пользователи" },
                new { Code = "permissions", Name = "Права доступа" }
            };

            foreach (var item in menuItems)
            {
                ExecuteSql(connection, @"
                    INSERT OR REPLACE INTO user_permissions 
                    (user_id, menu_item_code, can_read, can_write, can_edit, can_delete)
                    VALUES (@userId, @code, @read, @write, @edit, @delete)",
                    new SQLiteParameter("@userId", adminId),
                    new SQLiteParameter("@code", item.Code),
                    new SQLiteParameter("@read", true),
                    new SQLiteParameter("@write", true),
                    new SQLiteParameter("@edit", true),
                    new SQLiteParameter("@delete", true));
            }
        }

        private void ExecuteSql(SQLiteConnection connection, string sql, params SQLiteParameter[] parameters)
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
                cmd.ExecuteNonQuery();
            }
        }

        // Новый метод для принудительного создания таблиц при запуске
        public void EnsureTablesCreated()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Создаем таблицу system_users, если её нет
                    var checkUsersTable = "SELECT name FROM sqlite_master WHERE type='table' AND name='system_users'";
                    using (var cmd = new SQLiteCommand(checkUsersTable, connection))
                    {
                        var tableExists = cmd.ExecuteScalar() != null;

                        if (!tableExists)
                        {
                            CreateSystemUsersTable(connection);
                            Console.WriteLine("Таблица system_users создана");
                        }
                    }

                    // Создаем таблицу user_permissions, если её нет
                    var checkPermissionsTable = "SELECT name FROM sqlite_master WHERE type='table' AND name='user_permissions'";
                    using (var cmd = new SQLiteCommand(checkPermissionsTable, connection))
                    {
                        var tableExists = cmd.ExecuteScalar() != null;

                        if (!tableExists)
                        {
                            CreateUserPermissionsTable(connection);
                            Console.WriteLine("Таблица user_permissions создана");
                        }
                    }

                    // Проверяем наличие администратора
                    SeedDefaultAdmin(connection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании таблиц: {ex.Message}");
                throw;
            }
        }
    }
}
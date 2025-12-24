using System;
using System.Windows;
using BusParkManagementSystem.Data;

namespace BusParkManagementSystem
{
    public partial class App : Application
    {
        public static string ConnectionString { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Инициализируем DatabaseContext
                var dbContext = new DatabaseContext();
                ConnectionString = dbContext.GetConnectionString();

                // Выводим путь для отладки
                Console.WriteLine($"База данных находится по пути: {dbContext.GetDatabasePath()}");

                // Инициализация базы данных
                var migrator = new DatabaseMigrator(ConnectionString);

                // Гарантируем создание всех таблиц
                migrator.EnsureTablesCreated();

                Console.WriteLine("База данных и таблицы инициализированы");

                // Дополнительная миграция для совместимости
                try
                {
                    migrator.MigrateAuthenticationTables();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Предупреждение при миграции: {ex.Message}");
                    // Игнорируем, так как таблицы уже созданы
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}\n" +
                               "Приложение будет закрыто.",
                               "Критическая ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
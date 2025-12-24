using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem
{
    public partial class UserPermissionsWindow : Window
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly User _user;
        private List<UserPermission> _permissions;

        public UserPermissionsWindow(User user)
        {
            InitializeComponent();

            _user = user;
            _permissionRepository = new PermissionRepository(App.ConnectionString);

            Loaded += UserPermissionsWindow_Loaded;
        }

        private async void UserPermissionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UsernameText.Text = _user.Username;
                RoleText.Text = _user.Role;

                // Проверяем существование таблицы
                bool tableExists = await _permissionRepository.EnsureTableExists();
                if (!tableExists)
                {
                    MessageBox.Show("Таблица прав доступа не найдена. Обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                await LoadPermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке прав: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async Task LoadPermissions()
        {
            // Получаем все возможные модули системы
            var menuItems = await _permissionRepository.GetMenuItemsAsync();

            // Получаем текущие права пользователя
            var userPermissions = await _permissionRepository.GetPermissionsForUserAsync(_user.Id);

            _permissions = new List<UserPermission>();

            foreach (var menuItem in menuItems)
            {
                var existingPermission = userPermissions.FirstOrDefault(p =>
                    p.MenuItemCode == menuItem.Code);

                _permissions.Add(new UserPermission
                {
                    UserId = _user.Id,
                    MenuItemCode = menuItem.Code,
                    MenuItemName = menuItem.Name,
                    CanRead = existingPermission?.CanRead ?? (_user.Role == "Администратор"),
                    CanWrite = existingPermission?.CanWrite ?? (_user.Role == "Администратор"),
                    CanEdit = existingPermission?.CanEdit ?? (_user.Role == "Администратор"),
                    CanDelete = existingPermission?.CanDelete ?? (_user.Role == "Администратор")
                });
            }

            PermissionsGrid.ItemsSource = _permissions;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var success = await _permissionRepository.UpdatePermissionsAsync(
                    _user.Id, _permissions);

                if (success)
                {
                    MessageBox.Show("Права успешно сохранены",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить права",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
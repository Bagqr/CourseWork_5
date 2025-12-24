using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusParkManagementSystem
{
    public partial class RegistrationWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;

        public RegistrationWindow()
        {
            InitializeComponent();
            _userRepository = new UserRepository(App.ConnectionString);
            _permissionRepository = new PermissionRepository(App.ConnectionString);

            // Инициализируем ComboBox
            InitializeRoleComboBox();
        }

        private void InitializeRoleComboBox()
        {
            try
            {
                // Сначала очищаем Items
                RoleComboBox.Items.Clear();

                // Заполняем ComboBox ролями
                var roles = new[]
                {
                    "Администратор",
                    "Директор",
                    "Менеджер по кадрам",
                    "Диспетчер",
                    "Бухгалтер",
                    "Инженер гаража",
                    "Водитель",
                    "Кондуктор"
                };

                foreach (var role in roles)
                {
                    RoleComboBox.Items.Add(role);
                }

                // Выбираем первый элемент
                if (RoleComboBox.Items.Count > 0)
                    RoleComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации ComboBox: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string selectedRole = RoleComboBox.SelectedItem as string;

            // Валидация
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedRole))
            {
                MessageBox.Show("Выберите роль пользователя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, существует ли пользователь
                bool exists = await _userRepository.CheckUsernameExistsAsync(username);
                if (exists)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаем нового пользователя
                var newUser = new User
                {
                    Username = username,
                    PasswordHash = password, // UserRepository сам захеширует
                    Role = selectedRole,
                    IsActive = true,
                    MustChangePassword = MustChangePasswordCheckBox.IsChecked ?? false
                };

                // Сохраняем в БД
                int userId = await _userRepository.CreateAsync(newUser);

                if (userId > 0)
                {
                    // Инициализируем права доступа для нового пользователя
                    await InitializeUserPermissions(userId, selectedRole);

                    MessageBox.Show("Пользователь успешно зарегистрирован!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось создать пользователя",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task InitializeUserPermissions(int userId, string role)
        {
            try
            {
                // Получаем все пункты меню
                var menuItems = await _permissionRepository.GetMenuItemsAsync();
                var permissions = new List<UserPermission>();

                // Определяем права по умолчанию в зависимости от роли
                bool isAdmin = role == "Администратор";
                bool canReadDefault = true; // Все могут читать
                bool canWriteDefault = isAdmin;
                bool canEditDefault = isAdmin;
                bool canDeleteDefault = isAdmin;

                foreach (var menuItem in menuItems)
                {
                    permissions.Add(new UserPermission
                    {
                        UserId = userId,
                        MenuItemCode = menuItem.Code,
                        MenuItemName = menuItem.Name,
                        CanRead = canReadDefault,
                        CanWrite = canWriteDefault,
                        CanEdit = canEditDefault,
                        CanDelete = canDeleteDefault
                    });
                }

                // Сохраняем права
                await _permissionRepository.UpdatePermissionsAsync(userId, permissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации прав: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
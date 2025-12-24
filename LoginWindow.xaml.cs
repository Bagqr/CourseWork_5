using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem
{
    public partial class LoginWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;

        public LoginWindow()
        {
            InitializeComponent();

            _userRepository = new UserRepository(App.ConnectionString);
            _permissionRepository = new PermissionRepository(App.ConnectionString);

            Loaded += (s, e) => UsernameTextBox.Focus();
            SetupEnterNavigation();
        }

        private void SetupEnterNavigation()
        {
            UsernameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    PasswordBox.Focus();
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    LoginButton_Click(null, null);
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                // Получаем пользователя из БД
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null || !UserRepository.VerifyPassword(password, user.PasswordHash))
                {
                    ShowError("Неверный логин или пароль");
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                    return;
                }

                if (!user.IsActive)
                {
                    ShowError("Учетная запись заблокирована");
                    return;
                }

                // Обновляем время последнего входа
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Сохраняем пользователя
                CurrentUser.User = user;

                // Загружаем права пользователя из БД
                await LoadUserPermissions(user.Id);

                // Проверяем необходимость смены пароля
                if (user.MustChangePassword)
                {
                    MessageBox.Show("Требуется смена пароля при первом входе",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    var changePasswordWindow = new ChangePasswordWindow(user.Username, true);
                    changePasswordWindow.Owner = this;

                    if (changePasswordWindow.ShowDialog() == true)
                    {
                        OpenMainWindow();
                    }
                    else
                    {
                        // Если пользователь закрыл окно смены пароля, выходим из системы
                        CurrentUser.User = null;
                        PasswordBox.Clear();
                        PasswordBox.Focus();
                    }
                }
                else
                {
                    OpenMainWindow();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при входе: {ex.Message}");
            }
        }

        private async Task LoadUserPermissions(int userId)
        {
            try
            {
                var permissions = await _permissionRepository.GetPermissionsForUserAsync(userId);
                CurrentUser.Permissions = permissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке прав: {ex.Message}");
                CurrentUser.Permissions = new List<UserPermission>();
            }
        }

        private void OpenMainWindow()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegistrationWindow();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow(null, false);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
    }
}
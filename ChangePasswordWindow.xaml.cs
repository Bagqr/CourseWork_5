using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly string _username;
        private readonly bool _isForcedChange;
        private readonly IUserRepository _userRepository;

        public ChangePasswordWindow(string username = null, bool isForcedChange = false)
        {
            InitializeComponent();

            _username = username;
            _isForcedChange = isForcedChange;
            _userRepository = new UserRepository(App.ConnectionString);

            ConfigureUI();
            SetupEnterNavigation();
        }

        private void ConfigureUI()
        {
            // Настраиваем видимость полей в зависимости от контекста
            if (_isForcedChange)
            {
                // Принудительная смена пароля - не показываем поле старого пароля
                OldPasswordLabel.Visibility = Visibility.Collapsed;
                OldPasswordBox.Visibility = Visibility.Collapsed;

                // Если username передан, скрываем поле ввода логина
                if (!string.IsNullOrEmpty(_username))
                {
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameTextBox.Visibility = Visibility.Collapsed;
                }

                Title = "Требуется смена пароля";
            }
            else if (!string.IsNullOrEmpty(_username))
            {
                // Смена пароля администратором
                UsernameTextBox.Text = _username;
                UsernameTextBox.IsEnabled = false;
                OldPasswordLabel.Visibility = Visibility.Collapsed;
                OldPasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Самостоятельная смена пароля
                // Поля остаются видимыми по умолчанию
            }

            Loaded += (s, e) =>
            {
                if (UsernameTextBox.IsVisible && string.IsNullOrEmpty(UsernameTextBox.Text))
                    UsernameTextBox.Focus();
                else if (OldPasswordBox.IsVisible)
                    OldPasswordBox.Focus();
                else
                    NewPasswordBox.Focus();
            };
        }

        private void SetupEnterNavigation()
        {
            UsernameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    OldPasswordBox.Focus();
            };

            OldPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    NewPasswordBox.Focus();
            };

            NewPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    ConfirmPasswordBox.Focus();
            };

            ConfirmPasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    ChangeButton_Click(null, null);
            };
        }

        private string GetUsername()
        {
            if (!string.IsNullOrEmpty(_username))
            {
                return _username;
            }
            else
            {
                return UsernameTextBox.Text.Trim();
            }
        }

        private async void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            string username = GetUsername();
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (!await ValidateInput(username, oldPassword, newPassword, confirmPassword))
                return;

            try
            {
                // Получаем пользователя из базы данных
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    ShowError("Пользователь не найден");
                    return;
                }

                bool success;

                if (_isForcedChange)
                {
                    // Принудительная смена пароля - не проверяем старый пароль
                    success = await _userRepository.ResetPasswordAsync(user.Id, newPassword);

                    // Снимаем флаг необходимости смены пароля
                    if (success)
                    {
                        user.MustChangePassword = false;
                        await _userRepository.UpdateAsync(user);
                    }
                }
                else if (!string.IsNullOrEmpty(_username))
                {
                    // Смена пароля администратором (без проверки старого пароля)
                    success = await _userRepository.ResetPasswordAsync(user.Id, newPassword);
                }
                else
                {
                    // Самостоятельная смена (проверяем старый пароль)
                    success = await _userRepository.ChangePasswordAsync(user.Id, oldPassword, newPassword);
                    if (!success)
                    {
                        ShowError("Неверный текущий пароль");
                        OldPasswordBox.Clear();
                        OldPasswordBox.Focus();
                        return;
                    }
                }

                if (success)
                {
                    MessageBox.Show("Пароль успешно изменен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError("Не удалось изменить пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при изменении пароля: {ex.Message}");
            }
        }

        private async Task<bool> ValidateInput(string username, string oldPassword,
            string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Введите логин");
                UsernameTextBox.Focus();
                return false;
            }

            // Проверяем существование пользователя в базе данных
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                ShowError("Пользователь не найден");
                return false;
            }

            // Проверка старого пароля (если поле видимо и это не принудительная смена)
            if (OldPasswordBox.Visibility == Visibility.Visible && !_isForcedChange)
            {
                if (string.IsNullOrEmpty(oldPassword))
                {
                    ShowError("Введите текущий пароль");
                    OldPasswordBox.Focus();
                    return false;
                }

                // Проверяем старый пароль
                if (!UserRepository.VerifyPassword(oldPassword, user.PasswordHash))
                {
                    ShowError("Неверный текущий пароль");
                    OldPasswordBox.Clear();
                    OldPasswordBox.Focus();
                    return false;
                }
            }

            // Проверка нового пароля
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ShowError("Новый пароль должен содержать минимум 6 символов");
                NewPasswordBox.Clear();
                NewPasswordBox.Focus();
                return false;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("Новые пароли не совпадают");
                ConfirmPasswordBox.Clear();
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }

        private void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
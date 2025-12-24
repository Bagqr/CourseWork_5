using System;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            if (CurrentUser.IsAuthenticated && CurrentUser.User != null)
            {
                CurrentUserText.Text = $"{CurrentUser.User.Username} ({CurrentUser.User.Role})";
            }
            else
            {
                CurrentUserText.Text = "Гость";
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // В реальном приложении здесь будет логика смены языка
            MessageBox.Show("Язык интерфейса изменен. Для применения изменений перезапустите приложение.", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // В реальном приложении здесь будет логика смены темы
            MessageBox.Show("Тема оформления изменена. Для применения изменений перезапустите приложение.", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FontSizeIncrease_Click(object sender, RoutedEventArgs e)
        {
            // Увеличиваем размер шрифта
            double currentSize = double.Parse(FontSizeText.Text);
            if (currentSize < 24) // Максимальный размер
            {
                currentSize += 1;
                FontSizeText.Text = currentSize.ToString();
                FontSizeText.FontSize = currentSize;
            }
        }

        private void FontSizeDecrease_Click(object sender, RoutedEventArgs e)
        {
            // Уменьшаем размер шрифта
            double currentSize = double.Parse(FontSizeText.Text);
            if (currentSize > 8) // Минимальный размер
            {
                currentSize -= 1;
                FontSizeText.Text = currentSize.ToString();
                FontSizeText.FontSize = currentSize;
            }
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentPassword = CurrentPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                // Проверяем, что все поля заполнены
                if (string.IsNullOrEmpty(currentPassword) || 
                    string.IsNullOrEmpty(newPassword) || 
                    string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что новый пароль и подтверждение совпадают
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Новый пароль и подтверждение не совпадают.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем длину нового пароля
                if (newPassword.Length < 6)
                {
                    MessageBox.Show("Новый пароль должен содержать не менее 6 символов.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем текущий пароль
                if (!await CurrentUser.ValidatePasswordAsync(currentPassword))
                {
                    MessageBox.Show("Текущий пароль введен неверно.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Меняем пароль
                bool success = await CurrentUser.ChangePasswordAsync(currentPassword, newPassword);
                
                if (success)
                {
                    MessageBox.Show("Пароль успешно изменен.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Очищаем поля
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
                else
                {
                    MessageBox.Show("Ошибка при изменении пароля.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
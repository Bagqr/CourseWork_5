using System;
using System.Windows;

namespace BusParkManagementSystem
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnLanguage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Функция смены языка в разработке.\n" +
                    "Доступные языки: Русский, English", 
                    "Смена языка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене языка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFontSize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Функция изменения размера шрифта в разработке.\n" +
                    "Доступные размеры: Мелкий (10), Обычный (12), Крупный (16)", 
                    "Изменение размера шрифта", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении размера шрифта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем окно смены пароля
                var changePasswordWindow = new ChangePasswordWindow();
                changePasswordWindow.Owner = this;
                changePasswordWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна смены пароля: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Функция резервного копирования в разработке.\n" +
                    "Все данные будут сохранены в безопасное место.", 
                    "Резервное копирование", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при резервном копировании: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
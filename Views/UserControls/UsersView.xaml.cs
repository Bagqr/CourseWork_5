using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.Data;
using BusParkManagementSystem.Models;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class UsersView : UserControl
    {
        private readonly UserRepository _userRepository;
        private readonly UserPermissionsWindow _permissionsWindow;

        public UsersView()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
            _permissionsWindow = new UserPermissionsWindow();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                UsersGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = Window.GetWindow(this);
            if (registrationWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var changePasswordWindow = new ChangePasswordWindow(selectedUser);
            changePasswordWindow.Owner = Window.GetWindow(this);
            if (changePasswordWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить пользователя {selectedUser.Username}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _userRepository.DeleteUser(selectedUser.Id);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ManagePermissions_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для управления правами", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _permissionsWindow.SetUser(selectedUser);
            _permissionsWindow.Owner = Window.GetWindow(this);
            _permissionsWindow.ShowDialog();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}
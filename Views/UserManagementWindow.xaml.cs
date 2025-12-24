using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BusParkManagementSystem.Repositories;

namespace BusParkManagementSystem
{
    public partial class UserManagementWindow : Window
    {
        private readonly IUserRepository _userRepository;
        private List<User> _users;

        public UserManagementWindow()
        {
            InitializeComponent();
            _userRepository = new UserRepository(App.ConnectionString);
            Loaded += UserManagementWindow_Loaded;
        }

        private async void UserManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                _users = await _userRepository.GetAllAsync();
                UsersGrid.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this;
            registrationWindow.Closed += async (s, args) =>
            {
                // Обновляем список после закрытия окна регистрации
                await LoadUsers();
            };
            registrationWindow.ShowDialog();
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Редактирование пользователя - в разработке",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Нельзя удалить самого себя
            if (selectedUser.Id == CurrentUser.User?.Id)
            {
                MessageBox.Show("Нельзя удалить текущего пользователя",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{selectedUser.Username}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _userRepository.DeleteAsync(selectedUser.Id);
                    if (success)
                    {
                        MessageBox.Show("Пользователь успешно удален",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для сброса пароля",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var changePasswordWindow = new ChangePasswordWindow(selectedUser.Username);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }

        private void ManagePermissions_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для управления правами",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var permissionsWindow = new UserPermissionsWindow(selectedUser);
            permissionsWindow.Owner = this;
            permissionsWindow.ShowDialog();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
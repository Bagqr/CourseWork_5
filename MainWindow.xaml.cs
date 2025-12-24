using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.Views;
using BusParkManagementSystem.Views.UserControls;

namespace BusParkManagementSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем авторизацию
                if (!CurrentUser.IsAuthenticated)
                {
                    MessageBox.Show("Доступ запрещен. Пожалуйста, авторизуйтесь.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Показываем окно входа
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                    return;
                }

                // Обновляем информацию о пользователе
                UpdateUserInfo();

                // Настраиваем видимость вкладок в зависимости от прав
                await ConfigureTabVisibility();

                // Загружаем первую доступную вкладку
                LoadDefaultTab();

                StatusText.Text = "Система готова к работе";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке окна: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUserInfo()
        {
            if (CurrentUser.IsAuthenticated && CurrentUser.User != null)
            {
                CurrentUserText.Text = CurrentUser.User.Username;
                CurrentRoleText.Text = $"Роль: {CurrentUser.User.Role}";

                // Меняем иконку в зависимости от роли
                RoleIconText.Text = GetRoleIcon(CurrentUser.User.Role);
                AccessInfoText.Text = $"Доступ: {CurrentUser.User.Role}";
            }
            else
            {
                CurrentUserText.Text = "Гость";
                CurrentRoleText.Text = "Роль: Не авторизован";
                RoleIconText.Text = "👤";
                AccessInfoText.Text = "Доступ: ограничен";
            }
        }

        private string GetRoleIcon(string role)
        {
            switch (role)
            {
                case "Администратор": return "👑";
                case "Директор": return "💼";
                case "Менеджер по кадрам": return "👨‍💼";
                case "Диспетчер": return "🚦";
                case "Бухгалтер": return "💰";
                case "Инженер гаража": return "🔧";
                default: return "👤";
            }
        }

        private async Task ConfigureTabVisibility()
        {
            try
            {
                // Проверяем права для каждой вкладки и показываем/скрываем их
                TabBuses.Visibility = await CheckPermission("buses", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabRoutes.Visibility = await CheckPermission("routes", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabEmployees.Visibility = await CheckPermission("employees", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabTrips.Visibility = await CheckPermission("trips", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabReports.Visibility = await CheckPermission("reports", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabLookups.Visibility = await CheckPermission("lookups", "read") ? Visibility.Visible : Visibility.Collapsed;
                TabQueries.Visibility = await CheckPermission("queries", "read") ? Visibility.Visible : Visibility.Collapsed;

                // Вкладки для администратора
                TabUsers.Visibility = (CurrentUser.User?.Role == "Администратор") ? Visibility.Visible : Visibility.Collapsed;
                TabPermissions.Visibility = (CurrentUser.User?.Role == "Администратор") ? Visibility.Visible : Visibility.Collapsed;
                
                // Вкладка "Разное" - всегда видима для всех пользователей
                TabSettings.Visibility = Visibility.Visible;

                // Если все вкладки скрыты, показываем сообщение
                bool anyVisible = false;
                foreach (TabItem tabItem in MainTabControl.Items)
                {
                    if (tabItem.Visibility == Visibility.Visible)
                    {
                        anyVisible = true;
                        break;
                    }
                }

                if (!anyVisible)
                {
                    StatusText.Text = "Нет доступных модулей для вашей роли";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка настройки прав доступа: {ex.Message}";
            }
        }

        private void LoadDefaultTab()
        {
            // Загружаем первую доступную вкладку
            if (TabBuses.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabBuses;
                LoadTabContent(TabBuses);
            }
            else if (TabRoutes.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabRoutes;
                LoadTabContent(TabRoutes);
            }
            else if (TabEmployees.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabEmployees;
                LoadTabContent(TabEmployees);
            }
            else if (TabTrips.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabTrips;
                LoadTabContent(TabTrips);
            }
            else if (TabLookups.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabLookups;
                LoadTabContent(TabLookups);
            }
            else if (TabReports.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabReports;
                LoadTabContent(TabReports);
            }
            else if (TabQueries.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabQueries;
                LoadTabContent(TabQueries);
            }
            else if (TabSettings.Visibility == Visibility.Visible)
            {
                MainTabControl.SelectedItem = TabSettings;
                LoadTabContent(TabSettings);
            }
            else
            {
                MainContent.Content = new TextBlock
                {
                    Text = "Нет доступных модулей для вашей роли. Обратитесь к администратору.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap
                };
            }
        }

        private void LoadTabContent(TabItem tabItem)
        {
            try
            {
                if (tabItem == TabBuses)
                {
                    BusesFrame.Content = new BusesView();
                    StatusText.Text = "Модуль: Автобусы";
                }
                else if (tabItem == TabRoutes)
                {
                    RoutesFrame.Content = new RoutesView();
                    StatusText.Text = "Модуль: Маршруты";
                }
                else if (tabItem == TabEmployees)
                {
                    EmployeesFrame.Content = new EmployeesView();
                    StatusText.Text = "Модуль: Сотрудники";
                }
                else if (tabItem == TabTrips)
                {
                    TripsFrame.Content = new TripsView();
                    StatusText.Text = "Модуль: Рейсы";
                }
                else if (tabItem == TabReports)
                {
                    ReportsFrame.Content = new ReportsView();
                    StatusText.Text = "Модуль: Документы";
                }
                else if (tabItem == TabLookups)
                {
                    LookupsFrame.Content = new LookupMenuView();
                    StatusText.Text = "Модуль: Справочники";
                }
                else if (tabItem == TabQueries)
                {
                    QueriesFrame.Content = new QueriesView();
                    StatusText.Text = "Модуль: Запросы";
                }
                else if (tabItem == TabUsers)
                {
                    // Открываем окно управления пользователями
                    var userManagementWindow = new UserManagementWindow();
                    userManagementWindow.Owner = this;
                    userManagementWindow.ShowDialog();
                    StatusText.Text = "Управление пользователями";
                }
                else if (tabItem == TabPermissions)
                {
                    // Открываем окно управления правами
                    var permissionManagementWindow = new PermissionManagementWindow();
                    permissionManagementWindow.Owner = this;
                    permissionManagementWindow.ShowDialog();
                    StatusText.Text = "Управление правами доступа";
                }
                else if (tabItem == TabSettings)
                {
                    // Открываем окно настроек с подпунктами "Настройка" и "Сменить пароль"
                    var settingsView = new SettingsView();
                    SettingsFrame.Content = settingsView;
                    StatusText.Text = "Модуль: Разное";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке содержимого вкладки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem != null)
            {
                LoadTabContent((TabItem)MainTabControl.SelectedItem);
            }
        }

        // Обработчики кликов по кнопкам навигации - оставляем для совместимости
        private void BtnBuses_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabBuses;
            LoadTabContent(TabBuses);
        }

        private void BtnRoutes_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabRoutes;
            LoadTabContent(TabRoutes);
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabEmployees;
            LoadTabContent(TabEmployees);
        }

        private void BtnTrips_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabTrips;
            LoadTabContent(TabTrips);
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabReports;
            LoadTabContent(TabReports);
        }

        public void BtnLookups_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabLookups;
            LoadTabContent(TabLookups);
        }

        private void BtnQueries_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedItem = TabQueries;
            LoadTabContent(TabQueries);
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно управления пользователями
            var userManagementWindow = new UserManagementWindow();
            userManagementWindow.Owner = this;
            userManagementWindow.ShowDialog();
            StatusText.Text = "Управление пользователями";
        }

        private void BtnPermissions_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно управления правами
            var permissionManagementWindow = new PermissionManagementWindow();
            permissionManagementWindow.Owner = this;
            permissionManagementWindow.ShowDialog();
            StatusText.Text = "Управление правами доступа";
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new HelpWindow();
                helpWindow.Owner = this;
                helpWindow.Show();
                StatusText.Text = "Открыта справка";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии справки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Logout();
            }
        }

        private void Logout()
        {
            // Очищаем данные текущего пользователя
            CurrentUser.Logout();

            // Показываем окно входа
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // Закрываем главное окно
            this.Close();
        }

        private async Task<bool> CheckPermission(string menuCode, string accessType)
        {
            if (!CurrentUser.IsAuthenticated) return false;

            // Администратор имеет доступ ко всему
            if (CurrentUser.User?.Role == "Администратор") return true;

            // Проверяем права через CurrentUser
            return await CurrentUser.HasPermissionAsync(menuCode, accessType);
        }
        public void OpenLookup(string lookupType)
        {
            try
            {
                // Открываем соответствующее окно справочника
                switch (lookupType)
                {
                    case "Автобусы":
                        MainTabControl.SelectedItem = TabBuses;
                        LoadTabContent(TabBuses);
                        break;

                    case "Маршруты":
                        MainTabControl.SelectedItem = TabRoutes;
                        LoadTabContent(TabRoutes);
                        break;

                    case "Сотрудники":
                        MainTabControl.SelectedItem = TabEmployees;
                        LoadTabContent(TabEmployees);
                        break;

                    case "Рейсы":
                        MainTabControl.SelectedItem = TabTrips;
                        LoadTabContent(TabTrips);
                        break;

                    default:
                        // Если тип не найден, открываем общее окно справочников
                        MainTabControl.SelectedItem = TabLookups;
                        LoadTabContent(TabLookups);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии справочника '{lookupType}': {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Можно добавить дополнительные действия при закрытии окна
            // Например, сохранение настроек или проверку несохраненных данных
        }
    }
}
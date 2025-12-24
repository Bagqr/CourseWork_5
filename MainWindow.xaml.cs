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

                // Настраиваем видимость кнопок в зависимости от прав
                await ConfigureNavigationButtons();

                // Загружаем первую доступную страницу
                LoadDefaultPage();

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

        private async Task ConfigureNavigationButtons()
        {
            try
            {
                // Проверяем права для каждой кнопки и показываем/скрываем их
                BtnBuses.Visibility = await CheckPermission("buses", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnRoutes.Visibility = await CheckPermission("routes", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnEmployees.Visibility = await CheckPermission("employees", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnTrips.Visibility = await CheckPermission("trips", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnReports.Visibility = await CheckPermission("reports", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnLookups.Visibility = await CheckPermission("lookups", "read") ? Visibility.Visible : Visibility.Collapsed;
                BtnQueries.Visibility = await CheckPermission("queries", "read") ? Visibility.Visible : Visibility.Collapsed;

                // Кнопки для администратора
                BtnUsers.Visibility = (CurrentUser.User?.Role == "Администратор") ? Visibility.Visible : Visibility.Collapsed;
                BtnPermissions.Visibility = (CurrentUser.User?.Role == "Администратор") ? Visibility.Visible : Visibility.Collapsed;

                // Если все кнопки скрыты, показываем сообщение
                if (NavigationPanel.Children.Count > 0)
                {
                    bool anyVisible = false;
                    foreach (UIElement element in NavigationPanel.Children)
                    {
                        if (element is Button button && button.Visibility == Visibility.Visible)
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
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка настройки прав доступа: {ex.Message}";
            }
        }

        private async Task<bool> CheckPermission(string menuCode, string accessType)
        {
            if (!CurrentUser.IsAuthenticated) return false;

            // Администратор имеет доступ ко всему
            if (CurrentUser.User?.Role == "Администратор") return true;

            // Проверяем права через CurrentUser
            return await CurrentUser.HasPermissionAsync(menuCode, accessType);
        }

        private void LoadDefaultPage()
        {
            // Загружаем первую доступную страницу
            if (BtnBuses.Visibility == Visibility.Visible)
            {
                BtnBuses_Click(null, null);
            }
            else if (BtnRoutes.Visibility == Visibility.Visible)
            {
                BtnRoutes_Click(null, null);
            }
            else if (BtnEmployees.Visibility == Visibility.Visible)
            {
                BtnEmployees_Click(null, null);
            }
            else if (BtnTrips.Visibility == Visibility.Visible)
            {
                BtnTrips_Click(null, null);
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

        // Обработчики кликов по кнопкам навигации
        private void BtnBuses_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new BusesView();
                StatusText.Text = "Модуль: Автобусы";
                HighlightActiveButton(BtnBuses);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля автобусов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRoutes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new RoutesView();
                StatusText.Text = "Модуль: Маршруты";
                HighlightActiveButton(BtnRoutes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля маршрутов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new EmployeesView();
                StatusText.Text = "Модуль: Сотрудники";
                HighlightActiveButton(BtnEmployees);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля сотрудников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTrips_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new TripsView();
                StatusText.Text = "Модуль: Рейсы";
                HighlightActiveButton(BtnTrips);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля рейсов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new ReportsView();
                StatusText.Text = "Модуль: Отчёты";
                HighlightActiveButton(BtnReports);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля отчётов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BtnLookups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new LookupMenuView();
                StatusText.Text = "Модуль: Справочники";
                HighlightActiveButton(BtnLookups);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля справочников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnQueries_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainContent.Content = new QueriesView();
                StatusText.Text = "Модуль: Запросы";
                HighlightActiveButton(BtnQueries);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке модуля запросов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем окно управления пользователями
                var userManagementWindow = new UserManagementWindow();
                userManagementWindow.Owner = this;
                userManagementWindow.ShowDialog();
                StatusText.Text = "Управление пользователями";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии управления пользователями: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPermissions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Открываем окно управления правами
                var permissionManagementWindow = new PermissionManagementWindow();
                permissionManagementWindow.Owner = this;
                permissionManagementWindow.ShowDialog();
                StatusText.Text = "Управление правами доступа";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии управления правами: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void HighlightActiveButton(Button activeButton)
        {
            // Сбрасываем стили всех кнопок
            foreach (var element in NavigationPanel.Children)
            {
                if (element is Button button)
                {
                    button.Style = (Style)FindResource("PrimaryButton");
                }
            }

            // Выделяем активную кнопку
            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("SuccessButton");
            }
        }
        public void OpenLookup(string lookupType)
        {
            try
            {
                // Открываем соответствующее окно справочника
                switch (lookupType)
                {
                    case "Автобусы":
                        MainContent.Content = new BusesView();
                        StatusText.Text = "Справочник: Автобусы";
                        HighlightActiveButton(null); // Сброс выделения, так как это не кнопка навигации
                        break;

                    case "Маршруты":
                        MainContent.Content = new RoutesView();
                        StatusText.Text = "Справочник: Маршруты";
                        HighlightActiveButton(null);
                        break;

                    case "Сотрудники":
                        MainContent.Content = new EmployeesView();
                        StatusText.Text = "Справочник: Сотрудники";
                        HighlightActiveButton(null);
                        break;

                    case "Рейсы":
                        MainContent.Content = new TripsView();
                        StatusText.Text = "Справочник: Рейсы";
                        HighlightActiveButton(null);
                        break;

                    default:
                        // Если тип не найден, открываем общее окно справочников
                        var lookupView = new LookupView();
                        MainContent.Content = lookupView;
                        StatusText.Text = $"Справочник: {lookupType}";
                        HighlightActiveButton(null);
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
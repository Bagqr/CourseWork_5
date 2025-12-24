using System.Windows;
using System.Windows.Controls;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class LookupView : UserControl
    {
        public LookupView()
        {
            InitializeComponent();
        }

        // Обработчики для кнопок (если они есть в XAML)
        private void BusesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Автобусы");
        }

        private void RoutesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Маршруты");
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Сотрудники");
        }

        private void TripsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Рейсы");
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Модели автобусов");
        }

        private void PositionsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Должности");
        }

        private void StreetsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Улицы");
        }

        private void ColorsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Цвета");
        }

        private void StatesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Состояния автобусов");
        }

        private void ShiftTypesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Типы смен");
        }

        private void EventTypesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLookup("Типы кадровых мероприятий");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся в главное меню
            var window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                mainWindow.MainContent.Content = null; // или mainWindow.BtnLookups_Click(null, null);
            }
        }

        private void OpenLookup(string lookupType)
        {
            // Находим главное окно и вызываем его метод OpenLookup
            var window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                mainWindow.OpenLookup(lookupType);
            }
        }
    }
}
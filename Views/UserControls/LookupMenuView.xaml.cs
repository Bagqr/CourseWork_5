using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    /// <summary>
    /// Меню справочников с карточками для навигации
    /// </summary>
    public partial class LookupMenuView : UserControl
    {
        public LookupMenuView()
        {
            InitializeComponent();
        }

        // Метод для открытия конкретного справочника
        private void OpenLookup(string lookupType, string lookupName)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    // Вызываем метод открытия справочника из MainWindow
                    mainWindow.OpenLookup(lookupType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия справочника: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики кликов по карточкам
        private void ModelCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("Model", "Модели автобусов");
        }

        private void ColorCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("Color", "Цвета автобусов");
        }

        private void StateCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("BusState", "Состояния автобусов");
        }

        private void PositionCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("Position", "Должности сотрудников");
        }

        private void ShiftCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("ShiftType", "Типы смен");
        }

        private void StreetCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("Street", "Улицы");
        }

        private void StopCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("BusStop", "Остановки");
        }

        private void EventCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                OpenLookup("PersonnelEventType", "Кадровые мероприятия");
        }
    }
}
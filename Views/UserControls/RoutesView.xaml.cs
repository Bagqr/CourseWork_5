using System;
using System.Windows;
using System.Windows.Controls;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class RoutesView : UserControl
    {
        public RoutesView()
        {
            try
            {
                InitializeComponent();

                // Принудительно устанавливаем ViewModel в code-behind
                DataContext = new RouteViewModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации RoutesView: {ex.Message}");
            }
        }
    }
}
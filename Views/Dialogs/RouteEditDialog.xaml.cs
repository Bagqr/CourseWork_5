// Views\Dialogs\RouteEditDialog.xaml.cs
using System.Windows;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.Dialogs
{
    public partial class RouteEditDialog : Window
    {
        public RouteEditDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RouteEditViewModel viewModel)
            {
                viewModel.CloseAction = result => DialogResult = result;
            }
        }
    }
}
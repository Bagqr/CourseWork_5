using System.Windows;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.Dialogs
{
    public partial class BusEditDialog : Window
    {
        public BusEditDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BusEditViewModel viewModel)
            {
                viewModel.CloseAction = result => DialogResult = result;
            }
        }
    }
}

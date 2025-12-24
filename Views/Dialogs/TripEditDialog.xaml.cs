using System.Windows;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.Dialogs
{
    public partial class TripEditDialog : Window
    {
        public TripEditDialog()
        {
            InitializeComponent();

            Loaded += (sender, e) =>
            {
                if (DataContext is TripEditViewModel viewModel)
                {
                    viewModel.CloseAction = result => DialogResult = result;
                }
            };
        }
    }
}
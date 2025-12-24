using System.Windows.Controls;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class TripsView : UserControl
    {
        public TripsView()
        {
            InitializeComponent();
            DataContext = new TripViewModel();
        }
    }
}
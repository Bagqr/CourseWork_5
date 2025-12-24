using System.Windows.Controls;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class BusesView : UserControl
    {
        public BusesView()
        {
            InitializeComponent();
            DataContext = new BusViewModel();
        }
    }
}
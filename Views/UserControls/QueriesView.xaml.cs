using System.Windows.Controls;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class QueriesView : UserControl
    {
        public QueriesView()
        {
            InitializeComponent();
            DataContext = new QueryViewModel();
        }
    }
}
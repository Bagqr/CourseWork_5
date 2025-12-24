using System.Windows;
using System.Windows.Controls;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class AccessDeniedView : UserControl
    {
        public AccessDeniedView()
        {
            InitializeComponent();

            // Исправляем обращение к Role
            if (CurrentUser.IsAuthenticated && CurrentUser.User != null)
            {
                // Если в XAML есть TextBlock с именем RoleText, раскомментируйте:
                // RoleText.Text = CurrentUser.User.Role;
            }
        }

        // Добавляем недостающий метод
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Находим родительское окно
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (window is MainWindow mainWindow)
                {
                    // Если это главное окно, возвращаемся на главную
                    mainWindow.MainContent.Content = null; // или устанавливаем начальный вид
                }
                else
                {
                    // Если это отдельное окно, закрываем его
                    window.Close();
                }
            }
        }
    }
}
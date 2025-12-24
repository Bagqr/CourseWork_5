using System.Windows;
using System.Windows.Controls;

namespace BusParkManagementSystem.Views
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();

            // Устанавливаем стили кнопок при загрузке
            Loaded += HelpWindow_Loaded;
        }

        private void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Активируем первую кнопку (Содержание)
            SetActiveButton(BtnContent);
        }

        private void SetActiveButton(Button activeButton)
        {
            // Сброс всех кнопок к обычному стилю
            BtnContent.Style = (Style)FindResource("PrimaryButton");
            BtnAbout.Style = (Style)FindResource("PrimaryButton");

            // Установка активного стиля для выбранной кнопки
            activeButton.Style = (Style)FindResource("SuccessButton");

            // Обновление текста в статус-баре
            HelpSectionText.Text = $"Раздел: {activeButton.Content}";
        }

        private void BtnContent_Click(object sender, RoutedEventArgs e)
        {
            // Показываем содержание, скрываем "О программе"
            ContentScrollViewer.Visibility = Visibility.Visible;
            AboutScrollViewer.Visibility = Visibility.Collapsed;

            // Обновляем активную кнопку
            SetActiveButton(BtnContent);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            // Показываем "О программе", скрываем содержание
            ContentScrollViewer.Visibility = Visibility.Collapsed;
            AboutScrollViewer.Visibility = Visibility.Visible;

            // Обновляем активную кнопку
            SetActiveButton(BtnAbout);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Статический метод для удобного открытия окна
        public static void ShowHelp(Window owner = null)
        {
            var helpWindow = new HelpWindow();
            if (owner != null)
            {
                helpWindow.Owner = owner;
                helpWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            helpWindow.Show();
        }
    }
}
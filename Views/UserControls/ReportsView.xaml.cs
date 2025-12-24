using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BusParkManagementSystem.ViewModels;

namespace BusParkManagementSystem.Views.UserControls
{
    public partial class ReportsView : UserControl
    {
        private ReportViewModel _viewModel;

        public ReportsView()
        {
            InitializeComponent();
            _viewModel = new ReportViewModel();
            DataContext = _viewModel;
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.GenerateReportAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            try
            {
                // Преобразуем названия столбцов из CamelCase в читаемый формат
                var originalHeader = e.PropertyName;
                var displayHeader = ConvertCamelCaseToDisplay(originalHeader);
                e.Column.Header = displayHeader;

                // Настраиваем выравнивание для числовых столбцов
                if (IsNumericColumn(originalHeader))
                {
                    var textColumn = e.Column as DataGridTextColumn;
                    if (textColumn != null)
                    {
                        var style = new Style(typeof(TextBlock));
                        style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                        style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0)));
                        textColumn.ElementStyle = style;
                    }
                }

                // Устанавливаем ширину для длинных столбцов
                if (originalHeader.Contains("Выручка") || originalHeader.Contains("ИтогоКВыплате"))
                {
                    e.Column.Width = new DataGridLength(120);
                }
                else if (originalHeader.Contains("Дата"))
                {
                    e.Column.Width = new DataGridLength(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации колонки: {ex.Message}");
            }
        }

        private string ConvertCamelCaseToDisplay(string camelCase)
        {
            if (string.IsNullOrEmpty(camelCase))
                return camelCase;

            var result = new StringBuilder();
            result.Append(camelCase[0]);

            for (int i = 1; i < camelCase.Length; i++)
            {
                if (char.IsUpper(camelCase[i]))
                {
                    result.Append(' ');
                }
                result.Append(camelCase[i]);
            }

            // Специальные замены
            var text = result.ToString();
            text = text.Replace("Гос Номер", "Гос. номер");
            text = text.Replace("Количество Рейсов", "Количество рейсов");
            text = text.Replace("Плановая Выручка", "Плановая выручка");
            text = text.Replace("Фактическая Выручка", "Фактическая выручка");
            text = text.Replace("Средняя Выручка За Рейс", "Средняя выручка за рейс");
            text = text.Replace("Номер Маршрута", "Номер маршрута");
            text = text.Replace("Итого К Выплате", "Итого к выплате");

            return text;
        }

        private bool IsNumericColumn(string columnName)
        {
            return columnName.Contains("Выручка") ||
                   columnName.Contains("Оклад") ||
                   columnName.Contains("Премия") ||
                   columnName.Contains("ИтогоКВыплате") ||
                   columnName.Contains("Пробег") ||
                   columnName.Contains("Отклонение");
        }

        private void ReportDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                // Подсвечиваем строки с отрицательными отклонениями
                if (_viewModel.SelectedReportType == "Выручка по дням")
                {
                    var row = e.Row;
                    if (row.Item is System.Data.DataRowView dataRowView)
                    {
                        var отклонение = dataRowView["Отклонение"];
                        if (отклонение != null && отклонение != DBNull.Value)
                        {
                            if (Convert.ToDecimal(отклонение) < 0)
                            {
                                row.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)); // Красный фон
                            }
                            else if (Convert.ToDecimal(отклонение) > 0)
                            {
                                row.Background = new SolidColorBrush(Color.FromArgb(30, 0, 255, 0)); // Зеленый фон
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки форматирования
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Автоматически генерируем отчет при загрузке
            if (_viewModel != null)
            {
                _ = _viewModel.GenerateReportAsync();
            }
        }
    }
}
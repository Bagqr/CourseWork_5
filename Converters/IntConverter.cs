using System;
using System.Globalization;
using System.Windows.Data;

namespace BusParkManagementSystem.Converters
{
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование int в string для отображения
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование string в int
            string stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
                return 0;

            if (int.TryParse(stringValue, out int result))
                return result;

            return 0; // Возвращаем 0 если не удалось распарсить
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace BusParkManagementSystem.Converters
{
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование decimal в string для отображения
            if (value is decimal decimalValue)
                return decimalValue.ToString(CultureInfo.CurrentCulture);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразование string в decimal
            string stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
                return 0m;

            // Пробуем распарсить с учетом текущей культуры
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                return result;

            return 0m; // Возвращаем 0 если не удалось распарсить
        }
    }
}
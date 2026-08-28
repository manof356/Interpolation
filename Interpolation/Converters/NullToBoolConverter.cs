using System;
using System.Globalization;
using System.Windows.Data;

namespace Interpolation.Converters
{
    // Превращает значение ячейки результата в bool для IsEnabled кнопки:
    // null или пустая строка -> false (кнопка выключена), что угодно ещё -> true
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        // Обратное преобразование не нужно — только читаем значение
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
using Interpolation.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Interpolation.Converters
{
    // Конвертер статуса строки в иконку для row header
    public class RowStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not RowStatus status)
                return null;

            // подбираем имя ресурса-иконки по статусу
            string resourceKey = status switch
            {
                RowStatus.Error => "ErrorIcon",
                RowStatus.Warning => "WarningIcon",
                RowStatus.Info => "InfoIcon",
                _ => null // None — иконки нет
            };

            var resource = resourceKey == null ? null : Application.Current.TryFindResource(resourceKey);
            return resource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
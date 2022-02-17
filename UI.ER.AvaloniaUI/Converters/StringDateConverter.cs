using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UI.ER.AvaloniaUI.Converters
{

    public class StringDateConverter : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var date = (DateTime?)value;

            return date?.ToString("yyyy.MM.dd") ?? "";

        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            DateTime result = default;

            var valueTxt = value?.ToString()?.Trim() ?? "";

            bool success = DateTime.TryParseExact(valueTxt, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result);

            if (success)
                return result;

            return (DateTime?)null;
        }        
    }


}
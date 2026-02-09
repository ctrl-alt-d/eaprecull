using System;
using System.Globalization;

namespace UI.ER.ViewModels.Services
{
    public static class StringDateConverter
    {
        public static string Convert(DateTime? value)
        {
            var date = (DateTime?)value;

            return date?.ToString("dd.MM.yyyy") ?? "";

        }

        public static DateTime? ConvertBack(string? value)
        {
            var valueTxt = value?.ToString()?.Trim() ?? "";

            bool success = DateTime.TryParseExact(valueTxt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime result);

            if (success)
                return result;

            return (DateTime?)null;
        }

        public static bool NullableDataCorrecte(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var valueTxt = value?.ToString()?.Trim() ?? "";
            bool success = DateTime.TryParseExact(valueTxt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _);
            return success;
        }
    }

}



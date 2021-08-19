using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using System;
using System.Globalization;

namespace UI.ER.AvaloniaUI.Services
{
    public static class StringDateConverter
    {
        public static string Convert(DateTime? value)
        {
            var date = (DateTime?)value;

            return date?.ToString("d.M.yyyy") ?? "";

        }

        public static DateTime? ConvertBack(string value)
        {
            DateTime result = default;

            var valueTxt = value?.ToString()?.Trim() ?? "";

            bool success = DateTime.TryParseExact(valueTxt, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result);

            if (success)
                return result;

            return (DateTime?)null;
        }

        public static bool NullableDataCorrecte(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            bool success = DateTime.TryParseExact(value, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _);
            return success;
        }
    }

}



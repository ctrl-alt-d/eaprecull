using System;
using System.Globalization;

namespace UI.ER.ViewModels.Services
{
    public static class StringIntConverter
    {
        public static string Convert(int value)
        {
            return value.ToString();

        }

        public static int ConvertBack(string value)
        {
            var valueTxt = value?.ToString()?.Trim() ?? "";

            bool success = Int32.TryParse(valueTxt, out int result);

            if (success)
                return result;

            return default;
        }

        public static bool IntCorrecte(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var valueTxt = value?.ToString()?.Trim() ?? "";

            bool success = Int32.TryParse(valueTxt, out _);
            return success;
        }
    }

}



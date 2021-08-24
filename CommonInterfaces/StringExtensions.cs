
using System;

namespace CommonInterfaces
{
    public static class StringExtensions
    {
        public static string Left(this string str, int length)
        {
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string LeftAmbPuntsSuspensius(this string str, int length)
        {
            var s = str.Replace("\n", " / ");
            if (length < str.Length) 
                return s;
            
            return s.Substring(0, length) +  " ...";
        }


        public static string Right(this string str, int length)
        {
            return str.Substring(str.Length - Math.Min(length, str.Length));
        }
    }
}
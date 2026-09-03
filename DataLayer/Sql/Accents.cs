using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace DataLayer.Sql
{
    /// <summary>
    /// Treu els signes diacrítics d'un text: à→a, ï→i, ç→c, ñ→n.
    /// </summary>
    /// <remarks>
    /// És la implementació que hi ha darrere de la funció SQL
    /// <c>sense_accents()</c>, i també la que ha de normalitzar el text que
    /// busca l'usuari: si només s'aplica a un dels dos costats de la comparació,
    /// no lliga res.
    /// </remarks>
    internal static class Accents
    {
        /// <summary>
        /// Descompon el text (é → e + accent combinant) i llença les marques
        /// que no ocupen espai, que és on han anat a parar els diacrítics.
        /// </summary>
        [return: NotNullIfNotNull(nameof(text))]
        public static string? Treu(string? text)
        {
            if (text is null)
                return null;

            var descompost = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompost.Length);

            foreach (var caracter in descompost)
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                    sb.Append(caracter);

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}

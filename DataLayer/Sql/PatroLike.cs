using System.Text;

namespace DataLayer.Sql
{
    /// <summary>
    /// Converteix el text que escriu l'usuari en un patró de <c>LIKE</c>.
    /// </summary>
    internal static class PatroLike
    {
        /// <summary>El caràcter d'escapada del patró, declarat amb <c>ESCAPE</c>.</summary>
        internal const char Escapada = '\\';

        /// <summary>
        /// El patró que cerca el text a qualsevol posició: sense accents i entre
        /// comodins.
        /// </summary>
        /// <remarks>
        /// El text de l'usuari s'escapa: un <c>%</c> o un <c>_</c> escrits al cercador
        /// són el que semblen i no pas comodins, que és el que espera qui els escriu.
        /// </remarks>
        internal static string Conte(string? text)
        {
            var net = Accents.Treu(text) ?? string.Empty;
            var sb = new StringBuilder(net.Length + 2);

            sb.Append('%');

            foreach (var caracter in net)
            {
                if (caracter is Escapada or '%' or '_')
                    sb.Append(Escapada);

                sb.Append(caracter);
            }

            sb.Append('%');

            return sb.ToString();
        }
    }
}

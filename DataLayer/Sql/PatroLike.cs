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
        /// El patró que cerca el text a qualsevol posició: reduït a clau de cerca i
        /// entre comodins.
        /// </summary>
        /// <remarks>
        /// El text de l'usuari s'escapa: un <c>%</c> o un <c>_</c> escrits al cercador
        /// són el que semblen i no pas comodins, que és el que espera qui els escriu.
        /// <para>
        /// Un text que es queda sense res —algú que només ha escrit signes de
        /// puntuació— dóna <c>%%</c>, que ho troba tot: el terme no filtra, en comptes
        /// de no trobar ningú. És la lectura amable, i la volguda.
        /// </para>
        /// </remarks>
        internal static string Conte(string? text)
        {
            var clau = ClauDeCerca.De(text) ?? string.Empty;
            var sb = new StringBuilder(clau.Length + 2);

            sb.Append('%');

            foreach (var caracter in clau)
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

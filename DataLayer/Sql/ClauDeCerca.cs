using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataLayer.Sql
{
    /// <summary>
    /// Redueix un text a la forma amb què es compara a les cerques: minúscules, sense
    /// diacrítics i sense els signes que l'usuari escriu de maneres diferents volent
    /// dir el mateix.
    /// </summary>
    /// <remarks>
    /// Passen per aquí els dos costats de la comparació —la columna, via la funció SQL
    /// <c>clau_cerca</c>, i el text cercat, dins del patró—, i per això la clau no ha
    /// de ser bonica ni llegible: només ha de sortir igual per a les escriptures que
    /// volem donar per equivalents.
    /// <para>
    /// No és un «slug»: un slug d'URL substitueix els separadors per guionets i pot
    /// permetre's perdre lletres, i aquí perdre una lletra vol dir no trobar l'alumne.
    /// </para>
    /// </remarks>
    internal static class ClauDeCerca
    {
        /// <summary>
        /// Lletres que la descomposició Unicode no toca, perquè el traç i la ligatura
        /// en formen part i no són pas marques a banda. Sense aquesta taula,
        /// «Łukasz» no es troba escrivint «Lukasz».
        /// </summary>
        private static readonly Dictionary<char, string> Equivalencies = new()
        {
            ['ł'] = "l",    // polonès
            ['ø'] = "o",    // nòrdic
            ['đ'] = "d",    // croat, vietnamita
            ['ð'] = "d",    // islandès
            ['þ'] = "th",   // islandès
            ['ß'] = "ss",   // alemany
            ['æ'] = "ae",
            ['œ'] = "oe",
            ['ŀ'] = "l",    // ela geminada precomposada: «ŀl» i «l·l» han de coincidir
            ['ı'] = "i",    // turca sense punt
            ['ﬁ'] = "fi",   // les ligatures tipogràfiques arriben enganxades de PDF
            ['ﬂ'] = "fl",
            ['ﬀ'] = "ff",
            ['ﬃ'] = "ffi",
            ['ﬄ'] = "ffl",
        };

        /// <summary>
        /// Signes que desapareixen. Els agrupo per famílies del que l'usuari vol dir,
        /// no per blocs Unicode: dins de cada línia, tot vol dir el mateix.
        /// </summary>
        /// <remarks>
        /// La llista és tancada a posta. Esborrar «tota la puntuació» també tocaria les
        /// descripcions d'actuació, i allà no sabem què hi acabarà havent.
        /// </remarks>
        private const string Suprimits =
            "'’‘´`ʼʹ′" +  // apòstrofs: l'Anna, l’Anna, l´Anna
            "\"“”„«»″" +       // cometes, guillemets inclosos
            "-‐‑‒–—―−" +  // guions de tota mena
            "·‧.";                                 // punt volat: Paral·lel, Paral.lel, Parallel

        /// <summary>La clau de comparació del text, o <c>null</c> si no hi ha text.</summary>
        /// <remarks>
        /// L'ordre dels passos importa. Descompondre primer deixa els diacrítics com a
        /// marques a banda, que és el que permet llençar-los d'un sol cop; la taula
        /// s'aplica després, ja en minúscules, i així només li calen les entrades
        /// minúscules.
        /// </remarks>
        internal static string? De(string? text)
        {
            if (text is null)
                return null;

            var descompost = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompost.Length);

            foreach (var caracter in descompost)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

                // Els diacrítics, ara que la descomposició els ha deixat a part.
                if (categoria == UnicodeCategory.NonSpacingMark)
                    continue;

                // Guionet tou, amplada zero, marques de direcció: invisibles, i parteixen
                // la paraula per dins sense que ningú entengui per què no es troba.
                if (categoria == UnicodeCategory.Format)
                    continue;

                if (Suprimits.Contains(caracter))
                    continue;

                // Espai dur, espai fi... tots a espai normal.
                if (char.IsWhiteSpace(caracter))
                {
                    sb.Append(' ');
                    continue;
                }

                var minuscula = char.ToLowerInvariant(caracter);

                if (Equivalencies.TryGetValue(minuscula, out var equivalent))
                    sb.Append(equivalent);
                else
                    sb.Append(minuscula);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}

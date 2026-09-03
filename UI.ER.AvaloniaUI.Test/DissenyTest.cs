using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R6 — tots els colors de l'aplicació surten de <c>Themes/Paleta.axaml</c> o del
    /// MaterialTheme. Abans n'hi havia 95 escrits a pèl repartits per 13 fitxers, i per
    /// això no es podia canviar de tema. Aquests tests ho impedeixen: un
    /// <c>Background="#FAFAFA"</c> nou fa vermell aquí.
    /// </summary>
    /// <remarks>
    /// Escanegen el codi font, no l'assembly: un color literal compila igual de bé que
    /// una clau de recurs, i el que volem vigilar és justament què s'escriu a l'AXAML.
    /// </remarks>
    public class DissenyTest
    {
        // Claus que no són de la paleta pròpia perquè les posa el MaterialTheme.
        private static readonly string[] ClausDeMaterial = ["PrimaryHueMidBrush"];

        [Fact]
        public void CapAxamlEscriuUnColorLiteral()
        {
            var infraccions =
                (from fitxer in AxamlDeLaUI()
                 where Path.GetFileName(fitxer) != "Paleta.axaml"
                 from linia in File.ReadLines(fitxer).Select((text, i) => (text, num: i + 1))
                 where Regex.IsMatch(linia.text, "\"#[0-9A-Fa-f]{3,8}\"")
                    || Regex.IsMatch(linia.text, "(Foreground|Background|BorderBrush)=\"(White|Black|Gray|Red|Blue|Green)\"")
                 select $"{Path.GetFileName(fitxer)}:{linia.num}")
                .ToList();

            Assert.True(infraccions.Count == 0,
                "Colors escrits a pèl a l'AXAML. Cal una clau de Themes/Paleta.axaml: "
                + string.Join(", ", infraccions));
        }

        [Fact]
        public void CapCodiRereLaVistaConstrueixUnColor()
        {
            // Helpers/ConfirmationDialog muntava la UI en C# amb un SolidColorBrush literal.
            var infraccions =
                (from fitxer in Directory.EnumerateFiles(ArrelUI(), "*.cs", SearchOption.AllDirectories)
                 where !EsGenerat(fitxer)
                 from linia in File.ReadLines(fitxer).Select((text, i) => (text, num: i + 1))
                 where linia.text.Contains("Color.Parse(") || linia.text.Contains("new SolidColorBrush(")
                 select $"{Path.GetFileName(fitxer)}:{linia.num}")
                .ToList();

            Assert.True(infraccions.Count == 0,
                "Colors construïts en C#. La UI es descriu a l'AXAML: "
                + string.Join(", ", infraccions));
        }

        [Fact]
        public void ElsDosTemesDefineixenLesMateixesClaus()
        {
            var (clar, fosc) = ClausDeLaPaleta();

            Assert.NotEmpty(clar);
            Assert.Equal(clar.OrderBy(c => c), fosc.OrderBy(c => c));
        }

        [Fact]
        public void CadaClauQueSUsaExisteixAlaPaleta()
        {
            // Un DynamicResource que no resol no peta: el control es queda sense color i
            // ningú se n'assabenta fins que algú mira la pantalla.
            var definides = ClausDeLaPaleta().Clar.Concat(ClausDeMaterial).ToHashSet();

            var orfes =
                (from fitxer in AxamlDeLaUI()
                 from clau in Regex.Matches(File.ReadAllText(fitxer), @"\{DynamicResource ([A-Za-z0-9_]+)\}")
                                   .Select(m => m.Groups[1].Value)
                 where !definides.Contains(clau)
                 select $"{Path.GetFileName(fitxer)}: {clau}")
                .Distinct()
                .ToList();

            Assert.True(orfes.Count == 0,
                "DynamicResource sense definir: " + string.Join(", ", orfes));
        }

        [Fact]
        public void LaPaletaNoTeClausMortes()
        {
            var usades = AxamlDeLaUI()
                .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"\{DynamicResource ([A-Za-z0-9_]+)\}"))
                .Select(m => m.Groups[1].Value)
                .ToHashSet();

            var mortes = ClausDeLaPaleta().Clar.Where(c => !usades.Contains(c)).ToList();

            Assert.True(mortes.Count == 0,
                "Claus de la paleta que no fa servir ningú: " + string.Join(", ", mortes));
        }

        private static (List<string> Clar, List<string> Fosc) ClausDeLaPaleta()
        {
            var paleta = File.ReadAllText(Path.Combine(ArrelUI(), "Themes", "Paleta.axaml"));

            // Les dues taules van seguides dins de ResourceDictionary.ThemeDictionaries.
            var taules = Regex.Matches(paleta,
                    "<ResourceDictionary x:Key=\"(Light|Dark)\">(.*?)</ResourceDictionary>",
                    RegexOptions.Singleline)
                .ToDictionary(
                    m => m.Groups[1].Value,
                    m => Regex.Matches(m.Groups[2].Value, "x:Key=\"([A-Za-z0-9_]+)\"")
                              .Select(k => k.Groups[1].Value)
                              .ToList());

            return (taules["Light"], taules["Dark"]);
        }

        private static IEnumerable<string> AxamlDeLaUI()
            => Directory.EnumerateFiles(ArrelUI(), "*.axaml", SearchOption.AllDirectories)
                .Where(f => !EsGenerat(f));

        private static bool EsGenerat(string cami)
            => cami.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
            || cami.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}");

        /// <summary>
        /// Arrel de <c>UI.ER.AvaloniaUI</c> a partir d'on és aquest fitxer en temps de
        /// compilació. Evita endevinar quantes carpetes hi ha des de <c>bin/</c>.
        /// </summary>
        private static string ArrelUI([CallerFilePath] string aquestFitxer = "")
            => Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(aquestFitxer)!, "..", "UI.ER.AvaloniaUI"));
    }
}

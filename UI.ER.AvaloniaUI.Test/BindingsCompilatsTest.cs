using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R8 — cap AXAML es queda amb bindings per reflexió. R1 va posar
    /// <c>x:DataType</c> + <c>x:CompileBindings</c> als 14 fitxers que tenien
    /// <c>&lt;Design.DataContext&gt;</c>; R8 ha acabat els 19 restants.
    /// </summary>
    /// <remarks>
    /// El compilador ja peta si un binding compilat apunta a una propietat que no
    /// existeix (AVLN2000). El que no pot detectar és el pas enrere: un fitxer nou
    /// —o un d'existent que perdi els atributs— torna a la reflexió i els seus
    /// bindings deixen de comprovar-se en silenci. Això és el que vigilen aquests
    /// tests, i per això escanegen el codi font i no l'assembly.
    /// </remarks>
    public class BindingsCompilatsTest
    {
        /// <summary>
        /// Bindings contra el DataContext: <c>{Binding Nom}</c>,
        /// <c>{Binding X, Converter=...}</c>. En queden fora els que no en depenen
        /// —<c>RelativeSource</c>, <c>ElementName</c>, <c>$self</c>, <c>$parent</c>—
        /// i el <c>{Binding}</c> sense camí de les plantilles que iteren
        /// <c>string</c>, que no declara cap propietat.
        /// </summary>
        private static readonly Regex BindingAmbCami =
            new(@"\{Binding\s+(?!\$)[A-Za-z_][^}]*");

        private static bool LligaAlDataContext(string text)
            => BindingAmbCami.Matches(text).Any(m =>
                   !m.Value.Contains("RelativeSource") && !m.Value.Contains("ElementName"));

        [Fact]
        public void TotsElsAxamlDeclarenCompileBindings()
        {
            var sense = AxamlDeLaUI()
                .Where(f => !File.ReadAllText(f).Contains("x:CompileBindings=\"True\""))
                .Select(Path.GetFileName)
                .ToList();

            Assert.True(sense.Count == 0,
                "AXAML amb bindings per reflexió. Cal x:CompileBindings=\"True\" a l'element arrel: "
                + string.Join(", ", sense));
        }

        [Fact]
        public void CadaAxamlQueLligaAlDataContextDeclaraElSeuTipus()
        {
            // Sense x:DataType un {Binding Nom} no es pot comprovar contra res.
            var sense =
                (from fitxer in AxamlDeLaUI()
                 let text = File.ReadAllText(fitxer)
                 where LligaAlDataContext(text)
                 where !text.Contains("x:DataType=")
                 select Path.GetFileName(fitxer))
                .ToList();

            Assert.True(sense.Count == 0,
                "AXAML que lliga al DataContext sense dir de quin tipus és: "
                + string.Join(", ", sense));
        }

        [Fact]
        public void CapAxamlTornaAInstanciarElViewModelPerAlDissenyador()
        {
            // <Design.DataContext> instancia el ViewModel i per tant li exigeix
            // constructor sense paràmetres, que és el que R1 va treure (§R1.5).
            // El compilador no ho detecta —un bloc amb un VM que només té
            // constructor d'injecció compila igual, §R8.4—, així que aquest test
            // és l'única xarxa abans que peti el dissenyador.
            var infraccions = AxamlDeLaUI()
                .Where(f => File.ReadAllText(f).Contains("<Design.DataContext>"))
                .Select(Path.GetFileName)
                .ToList();

            Assert.True(infraccions.Count == 0,
                "<Design.DataContext> instancia el ViewModel i li exigeix constructor buit: "
                + string.Join(", ", infraccions));
        }

        private static IEnumerable<string> AxamlDeLaUI()
            => Directory.EnumerateFiles(ArrelUI(), "*.axaml", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                         && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        /// <inheritdoc cref="DissenyTest"/>
        private static string ArrelUI([CallerFilePath] string aquestFitxer = "")
            => Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(aquestFitxer)!, "..", "UI.ER.AvaloniaUI"));
    }
}

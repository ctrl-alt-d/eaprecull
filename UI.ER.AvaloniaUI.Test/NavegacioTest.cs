using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using UI.ER.AvaloniaUI.Views;
using UI.ER.ViewModels.ViewModels;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R4 — la navegació de l'aplicació viu a <see cref="AppStatusViewModel"/>, no al codi
    /// rere <see cref="MainWindow"/>. Aquests tests impedeixen que hi torni: qui afegeixi
    /// una entrada de menú amb un <c>Click=</c> que obri una finestra fa vermell aquí.
    /// </summary>
    public class NavegacioTest
    {
        private static readonly Type TaulellVm = typeof(AppStatusViewModel);

        // Handlers de Click que MainWindow conserva legítimament: cap obre una finestra.
        // Són ordres a l'aplicació sencera —tancar-la, canviar de tema—, no navegació,
        // i per tant no tenen ViewModel de destí a qui demanar-les.
        private static readonly string[] HandlersDeVista =
            ["SortirMenuItem_OnClick", "TemaMenuItem_OnClick"];

        [Fact]
        public void CadaLlistaDEntitatEsArribableDesDelTaulell()
        {
            // Per comprensió sobre les finestres reals: una entitat nova amb el seu
            // *SetWindow entra sola al test i obliga a donar-li entrada al menú.
            var sense = Vistes.Finestres
                .Where(v => v.Name.EndsWith("SetWindow", StringComparison.Ordinal))
                .Select(v => v.Name[..^"SetWindow".Length])
                .Where(entitat => Comanda($"{entitat}SetCommand") is null
                                  || Interaccio($"Show{entitat}SetDialog") is null)
                .ToList();

            Assert.True(sense.Count == 0,
                "Entitats sense {X}SetCommand + Show{X}SetDialog a AppStatusViewModel: "
                + string.Join(", ", sense));
        }

        [Fact]
        public void TotaInteraccioDelTaulellTeLaSevaComanda()
        {
            // Una Interaction sense comanda que la dispari només es pot llançar des del
            // code-behind: és exactament el que R4 ha tret.
            var orfes = TaulellVm.GetProperties()
                .Where(p => EsInteraccio(p.PropertyType))
                .Select(p => p.Name)
                .Where(nom => Comanda(NomDeComanda(nom)) is null)
                .ToList();

            Assert.True(orfes.Count == 0,
                "Interactions d'AppStatusViewModel sense la comanda corresponent: "
                + string.Join(", ", orfes));
        }

        [Fact]
        public void MainWindowNoNavegaAmbHandlersDeClick()
        {
            // Signatura exacta d'un handler de Click. Els altres handlers que hi queden
            // (KeyUp, PointerReleased, TemplateApplied) reben subtipus de RoutedEventArgs
            // i no compten.
            var handlers = typeof(MainWindow)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public
                            | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                // Els noms amb '<' són lambdes que el compilador ha convertit en
                // mètodes de la classe (la sincronització del calaix de navegació).
                .Where(m => !m.Name.StartsWith('<'))
                .Where(m => m.GetParameters() is [_, { } segon]
                            && segon.ParameterType == typeof(RoutedEventArgs))
                .Select(m => m.Name)
                .Except(HandlersDeVista)
                .ToList();

            Assert.True(handlers.Count == 0,
                "Handlers de Click al code-behind de MainWindow: " + string.Join(", ", handlers)
                + ". La navegació va per comanda del ViewModel + IWindowFactory.");
        }

        /// <summary>«ShowActuacioSetDialog» → «ActuacioSetCommand».</summary>
        private static string NomDeComanda(string interaccio)
            => interaccio.StartsWith("Show", StringComparison.Ordinal)
               && interaccio.EndsWith("Dialog", StringComparison.Ordinal)
                ? interaccio["Show".Length..^"Dialog".Length] + "Command"
                : interaccio + "Command";

        private static PropertyInfo? Comanda(string nom)
            => TaulellVm.GetProperty(nom) is { } p
               && typeof(ICommand).IsAssignableFrom(p.PropertyType)
                ? p
                : null;

        private static PropertyInfo? Interaccio(string nom)
            => TaulellVm.GetProperty(nom) is { } p && EsInteraccio(p.PropertyType) ? p : null;

        private static bool EsInteraccio(Type t)
            => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Interaction<,>);
    }
}

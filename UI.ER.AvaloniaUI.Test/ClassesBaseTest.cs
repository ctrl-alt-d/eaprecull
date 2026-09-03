using System;
using System.Linq;
using System.Reflection;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.ViewModels.ViewModels.Base;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R3 — el boilerplate dels diàlegs viu a quatre classes base. Aquests tests
    /// impedeixen que hi torni: una vista nova que es faci la seva pròpia versió del
    /// patró fa vermell aquí, no pas d'aquí a sis mesos quan algú compti línies.
    /// </summary>
    public class ClassesBaseTest
    {
        // Vistes que legítimament no segueixen cap dels tres patrons d'entitat.
        private static readonly string[] Excepcions =
        [
            "MainWindow",                  // navegació de l'app — R4
            "UtilitatsWindow",             // no edita cap entitat
            "AlumneInformeViewerWindow",   // visor, no diàleg d'edició — R2
        ];

        [Fact]
        public void ElsDialegsDEdicioHeretenDeEntityEditWindow()
            => AssertHeretaDe("Window", typeof(EntityEditWindow<,>),
                v => v.Name.EndsWith("CreateWindow") || v.Name.EndsWith("UpdateWindow"));

        [Fact]
        public void LesLlistesHeretenDeEntitySetWindow()
            => AssertHeretaDe("Window", typeof(EntitySetWindow<,,,>),
                v => v.Name.EndsWith("SetWindow"));

        [Fact]
        public void LesFilesHeretenDeEntityRowUserCtrl()
            => AssertHeretaDe("UserControl", typeof(EntityRowUserCtrl<,,,,>),
                v => v.Name.EndsWith("RowUserCtrl"));

        [Fact]
        public void LesLlistesHeretenDeSetViewModelBase()
        {
            var candidats = Vistes.ViewModels
                .Where(vm => vm.Name.EndsWith("SetViewModel", StringComparison.Ordinal))
                .ToList();

            Assert.NotEmpty(candidats);

            var fora = candidats
                .Where(vm => !HeretaDe(vm, typeof(SetViewModelBase<,>)))
                .Select(vm => vm.Name)
                .ToList();

            Assert.True(fora.Count == 0,
                "*SetViewModel que no hereten de SetViewModelBase: " + string.Join(", ", fora));
        }

        [Fact]
        public void CapLlistaEsFaElSeuPropiBucleDeCarrega()
        {
            // El que SetViewModelBase absorbeix: la col·lecció de files, l'estat de
            // càrrega i el missatge de paginació. Una llista que se'n torni a declarar
            // cap d'aquests membres es queda fora del refresc silenciós sense que res
            // més ho digui.
            string[] absorbits = ["MyItems", "BrokenRules", "Loading", "PaginatedMsg"];

            var copies =
                (from vm in Vistes.ViewModels
                 where vm.Name.EndsWith("SetViewModel", StringComparison.Ordinal)
                 from membre in vm.GetMembers(BindingFlags.Instance | BindingFlags.Public
                                              | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                 where absorbits.Contains(membre.Name)
                 select $"{vm.Name}.{membre.Name}")
                .ToList();

            Assert.True(copies.Count == 0,
                "Llistes que es tornen a declarar el que absorbeix SetViewModelBase: "
                + string.Join(", ", copies));
        }

        [Fact]
        public void CapVistaEsFaLaSevaPropiaGetWindow()
        {
            // Substituïda per Helpers.VisualRootExtensions.GetOwnerWindow().
            var copies = Vistes.Totes
                .Where(v => v.GetMethod("GetWindow",
                            BindingFlags.Instance | BindingFlags.NonPublic
                            | BindingFlags.Public | BindingFlags.DeclaredOnly) is not null)
                .Select(v => v.Name)
                .ToList();

            Assert.True(copies.Count == 0,
                "Vistes amb una còpia pròpia de GetWindow(): " + string.Join(", ", copies));
        }

        [Fact]
        public void CapVistaObreLExploradorPelSeuCompte()
        {
            // Substituït per Helpers.FileExplorer.Obre().
            var copies = Vistes.Totes
                .Where(v => v.GetMethod("ObraFileExplorer",
                            BindingFlags.Instance | BindingFlags.NonPublic
                            | BindingFlags.Public | BindingFlags.DeclaredOnly) is not null)
                .Select(v => v.Name)
                .ToList();

            Assert.True(copies.Count == 0,
                "Vistes amb una còpia pròpia d'ObraFileExplorer(): " + string.Join(", ", copies));
        }

        private static void AssertHeretaDe(string sufixVista, Type classeBase, Func<Type, bool> filtre)
        {
            var candidates = Vistes.Totes
                .Where(filtre)
                .Where(v => !Excepcions.Contains(v.Name))
                .ToList();

            Assert.NotEmpty(candidates);

            var fora = candidates
                .Where(v => !HeretaDe(v, classeBase))
                .Select(v => v.Name)
                .ToList();

            Assert.True(fora.Count == 0,
                $"Vistes de tipus {sufixVista} que no hereten de {classeBase.Name}: "
                + string.Join(", ", fora));
        }

        private static bool HeretaDe(Type tipus, Type definicioGenerica)
        {
            for (var t = tipus.BaseType; t is not null; t = t.BaseType)
                if (t.IsGenericType && t.GetGenericTypeDefinition() == definicioGenerica)
                    return true;

            return false;
        }
    }
}

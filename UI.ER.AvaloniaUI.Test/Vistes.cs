using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using UI.ER.AvaloniaUI;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// Punt únic des d'on els tests enumeren les vistes i els ViewModels reals de
    /// l'aplicació. Cap test escriu llistes de tipus a mà: si s'afegeix una vista nova,
    /// entra sola a tots els tests.
    /// </summary>
    internal static class Vistes
    {
        public static Assembly AssemblyUI => typeof(App).Assembly;

        public static Assembly AssemblyViewModels => typeof(ViewModelBase).Assembly;

        public static IReadOnlyList<Type> Finestres { get; } =
            Instanciables(AssemblyUI, typeof(Window));

        public static IReadOnlyList<Type> Controls { get; } =
            Instanciables(AssemblyUI, typeof(UserControl));

        /// <summary>Tot allò que Avalonia pot arribar a instanciar: finestres i controls.</summary>
        public static IReadOnlyList<Type> Totes { get; } =
            Finestres.Concat(Controls).ToList();

        public static IReadOnlyList<Type> ViewModels { get; } =
            Instanciables(AssemblyViewModels, typeof(ViewModelBase));

        /// <summary>
        /// Els ViewModels que el contenidor pot construir sol, segons la regla que
        /// aplica <c>DI.Injection</c>: tenir un constructor amb tots els paràmetres
        /// amb valor per defecte.
        /// </summary>
        public static bool EsConstruiblePelContenidor(Type viewModel)
            => viewModel.GetConstructors()
                .Any(c => c.GetParameters().All(p => p.HasDefaultValue));

        private static List<Type> Instanciables(Assembly assembly, Type baseType)
            => assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && baseType.IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();
    }
}

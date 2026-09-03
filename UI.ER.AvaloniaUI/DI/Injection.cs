using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.DI
{
    /// <summary>
    /// Registre de la capa de presentació. Tot per comprensió: cap vista ni cap
    /// ViewModel s'enumera a mà.
    /// </summary>
    public static class Injection
    {
        public static IServiceCollection UIConfigureServices(this IServiceCollection services)
        {
            foreach (var view in Views())
                // Transient obligatori: un Window d'Avalonia tancat no es pot reobrir.
                services.AddTransient(view);

            foreach (var viewModel in ViewModelsSenseArgumentsDeRuntime())
                services.AddTransient(viewModel);

            services.AddSingleton<IWindowFactory, WindowFactory>();

            ValidaConvencioVistaViewModel();

            return services;
        }

        /// <summary>Totes les finestres de l'assembly de la UI.</summary>
        private static IEnumerable<Type> Views()
            => typeof(Injection).Assembly.GetTypes()
                .Where(t => !t.IsAbstract
                         && !t.IsGenericTypeDefinition
                         && typeof(Window).IsAssignableFrom(t));

        /// <summary>
        /// ViewModels que el contenidor pot construir sol. El filtre exclou
        /// automàticament els <c>{…}UpdateViewModel(int id)</c>, els <c>{…}RowViewModel</c>
        /// i l'<c>AlumneInformeViewerViewModel</c>: aquests sempre passen per
        /// <see cref="IWindowFactory.GetWith{TWindow}"/>.
        /// </summary>
        private static IEnumerable<Type> ViewModelsSenseArgumentsDeRuntime()
            => typeof(ViewModelBase).Assembly.GetTypes()
                .Where(t => !t.IsAbstract
                         && !t.IsGenericTypeDefinition
                         && typeof(ViewModelBase).IsAssignableFrom(t)
                         && t.GetConstructors()
                             .Any(c => c.GetParameters().All(p => p.HasDefaultValue)));

        /// <summary>
        /// Comprova a l'arrencada que cada finestra té ViewModel resoluble. Així,
        /// afegir una finestra fora de convenció i sense <see cref="ViewModelAttribute"/>
        /// peta immediatament i no en obrir el diàleg.
        /// </summary>
        private static void ValidaConvencioVistaViewModel()
        {
            var errors = Views()
                .Select(view =>
                {
                    try
                    {
                        WindowFactory.ViewModelTypeFor(view);
                        return null;
                    }
                    catch (InvalidOperationException ex)
                    {
                        return ex.Message;
                    }
                })
                .OfType<string>()
                .ToList();

            if (errors.Count > 0)
                throw new InvalidOperationException(
                    "Vistes sense ViewModel resoluble:"
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, errors));
        }
    }
}

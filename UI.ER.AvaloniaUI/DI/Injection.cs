using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.Services;
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

            // Scoped, i abans dels ViewModels: és la dependència que tots ells demanen
            // pel constructor, i és el registre que fa que l'scope per diàleg de la
            // IWindowFactory alliberi de debò les operacions de BL (R1).
            services.AddScoped<IServiceFactory, ServiceFactory>();

            foreach (var viewModel in ViewModelsSenseArgumentsDeRuntime(services))
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
        /// automàticament els <c>{…}UpdateViewModel(…, int id)</c>, els
        /// <c>{…}RowViewModel</c> i l'<c>AlumneInformeViewerViewModel</c>: aquests sempre
        /// passen per <see cref="IWindowFactory.GetWith{TWindow}"/> o per
        /// <see cref="IWindowFactory.Get{TWindow}"/> amb arguments.
        /// </summary>
        /// <remarks>
        /// Des de R1 la regla és «tots els paràmetres són resolubles», no «tots tenen valor
        /// per defecte»: els ViewModels reben l'<c>IServiceFactory</c> pel constructor i cap
        /// no compliria la regla antiga. Per això el registre de l'<c>IServiceFactory</c> ha
        /// d'anar abans d'aquest escaneig.
        /// </remarks>
        private static IEnumerable<Type> ViewModelsSenseArgumentsDeRuntime(IServiceCollection services)
            => typeof(ViewModelBase).Assembly.GetTypes()
                .Where(t => !t.IsAbstract
                         && !t.IsGenericTypeDefinition
                         && typeof(ViewModelBase).IsAssignableFrom(t)
                         && t.GetConstructors()
                             .Any(c => c.GetParameters().All(p => EsResoluble(p, services))));

        /// <summary>
        /// Un paràmetre de constructor és resoluble si el contenidor en sap el tipus o si
        /// té valor per defecte. Els arguments de runtime —un <c>int id</c>, un DTO— no
        /// compleixen ni l'una ni l'altra, i és així com queden fora del registre.
        /// </summary>
        private static bool EsResoluble(ParameterInfo parametre, IServiceCollection services)
            => parametre.HasDefaultValue
               || services.Any(d => d.ServiceType == parametre.ParameterType);

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

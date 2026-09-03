using System;
using System.Reflection;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Services
{
    /// <inheritdoc cref="IWindowFactory"/>
    public sealed class WindowFactory(IServiceProvider provider) : IWindowFactory
    {
        /// <summary>Espai de noms on viuen tots els ViewModels de l'aplicació.</summary>
        private static readonly string ViewModelsNamespace = typeof(ViewModelBase).Namespace!;

        public TWindow Get<TWindow>() where TWindow : Window
            => Build<TWindow>(scope =>
                (ViewModelBase)scope.ServiceProvider.GetRequiredService(ViewModelTypeFor(typeof(TWindow))));

        public TWindow GetWith<TWindow>(ViewModelBase dataContext) where TWindow : Window
        {
            // Validació de l'argument abans de construir res: si la parella no quadra,
            // no s'obre cap scope ni s'instancia cap finestra.
            VerificaParella(typeof(TWindow), dataContext);
            return Build<TWindow>(_ => dataContext);
        }

        private TWindow Build<TWindow>(Func<IServiceScope, ViewModelBase> dataContextFactory)
            where TWindow : Window
        {
            // Un scope per diàleg: els serveis transitoris IDisposable que es creïn
            // durant la seva vida s'alliberen en tancar-lo, en comptes d'acumular-se
            // al provider arrel fins que es tanca l'aplicació.
            var scope = provider.CreateScope();
            try
            {
                var window = scope.ServiceProvider.GetRequiredService<TWindow>();
                window.DataContext = dataContextFactory(scope);
                window.Closed += (_, _) => scope.Dispose();
                return window;
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Comprova que el ViewModel rebut és el que la convenció assigna a la vista. Sense
        /// això, <c>GetWith</c> acceptaria qualsevol ViewModel i el desaparellament es
        /// manifestaria com un diàleg amb els bindings buits, sense cap error.
        /// </summary>
        private static void VerificaParella(Type viewType, ViewModelBase dataContext)
        {
            var esperat = ViewModelTypeFor(viewType);

            if (!esperat.IsInstanceOfType(dataContext))
                throw new ArgumentException(
                    $"«{viewType.Name}» espera un {esperat.Name} com a DataContext, "
                    + $"i se li ha passat un {dataContext.GetType().Name}.",
                    nameof(dataContext));
        }

        /// <summary>
        /// Determina el ViewModel d'una vista: primer per <see cref="ViewModelAttribute"/>,
        /// si no n'hi ha, per convenció de noms.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si la vista no segueix la convenció
        /// ni declara l'atribut.</exception>
        public static Type ViewModelTypeFor(Type viewType)
        {
            if (viewType.GetCustomAttribute<ViewModelAttribute>() is { } attr)
                return attr.ViewModelType;

            var baseName = RemoveSuffix(RemoveSuffix(viewType.Name, "Window"), "UserCtrl");
            var expected = $"{ViewModelsNamespace}.{baseName}ViewModel";

            return typeof(ViewModelBase).Assembly.GetType(expected)
                   ?? throw new InvalidOperationException(
                       $"No s'ha trobat el ViewModel per a «{viewType.Name}». " +
                       $"S'esperava «{expected}», o bé un [ViewModel(typeof(...))] a la vista.");
        }

        private static string RemoveSuffix(string value, string suffix)
            => value.EndsWith(suffix, StringComparison.Ordinal)
                ? value[..^suffix.Length]
                : value;
    }
}

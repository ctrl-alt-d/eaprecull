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

        public TWindow Get<TWindow>(params object[] vmArgs) where TWindow : Window
            => Build<TWindow>(scope =>
                CreaViewModel(scope.ServiceProvider, ViewModelTypeFor(typeof(TWindow)), vmArgs));

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
        /// Construeix el ViewModel des d'un proveïdor. Sense arguments de runtime surt del
        /// contenidor tal com el registra <c>DI.Injection</c>; amb arguments, els combina
        /// amb les dependències registrades — l'<c>IServiceFactory</c> de l'scope, en tots
        /// els casos d'avui.
        /// </summary>
        /// <remarks>
        /// És <c>public</c> i <c>static</c> perquè els tests hi arribin: és l'única part de
        /// la factory que no necessita una plataforma d'Avalonia per exercitar-se.
        /// <c>ActivatorUtilities</c> aparella cada argument amb un paràmetre del mateix
        /// tipus i omple la resta amb el contenidor o amb el valor per defecte. Els
        /// paràmetres nullables hi entren igual (<c>WindowFactoryTest</c> ho fixa), però
        /// l'aparellament és <em>per tipus</em>, no per posició: dos paràmetres del mateix
        /// tipus no es poden distingir, i per això la <c>ConfirmacioWindow</c> —tres
        /// <c>string</c> seguits— continua passant per <see cref="GetWith{TWindow}"/>.
        /// </remarks>
        public static ViewModelBase CreaViewModel(
            IServiceProvider provider, Type viewModelType, params object[] vmArgs)
            => (ViewModelBase)(vmArgs.Length == 0
                ? provider.GetRequiredService(viewModelType)
                : ActivatorUtilities.CreateInstance(provider, viewModelType, vmArgs));

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

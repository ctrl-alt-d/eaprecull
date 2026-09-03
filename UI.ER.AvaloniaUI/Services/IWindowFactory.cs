using Avalonia.Controls;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Services
{
    /// <summary>
    /// Construeix finestres amb el seu ViewModel resolt pel contenidor.
    /// Substitueix els <c>new XWindow { DataContext = new XViewModel() }</c> repartits
    /// pel codi de les vistes.
    /// </summary>
    public interface IWindowFactory
    {
        /// <summary>
        /// Resol la finestra i el seu ViewModel per convenció (o per
        /// <see cref="ViewModelAttribute"/>). Només per als ViewModels que es poden
        /// construir sense arguments de runtime.
        /// </summary>
        TWindow Get<TWindow>() where TWindow : Window;

        /// <summary>
        /// Resol la finestra i li assigna un ViewModel ja construït. És el cas dels
        /// ViewModels que arriben d'una <c>Interaction</c> i el dels que necessiten
        /// arguments de runtime (<c>modeLookup</c>, <c>id</c>, <c>alumneId</c>).
        /// </summary>
        /// <remarks>
        /// Es diferencia de <see cref="Get{TWindow}"/> només en <em>qui construeix</em> el
        /// ViewModel, no en el seu tipus: <paramref name="dataContext"/> ha de ser el
        /// ViewModel que la convenció assigna a <typeparamref name="TWindow"/>. Si no ho és,
        /// llança abans d'obrir el diàleg.
        /// </remarks>
        /// <exception cref="ArgumentException">Si el ViewModel no correspon a la finestra.</exception>
        TWindow GetWith<TWindow>(ViewModelBase dataContext) where TWindow : Window;
    }
}

using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// Base de tots els ViewModels. Implementa <see cref="IActivatableViewModel"/> perquè
    /// el <c>WhenActivated</c> de la vista activi també el del ViewModel: així una
    /// subscripció feta al constructor mor en tancar-se la finestra sense que cap vista
    /// hagi de saber-ne res, i tant si el ViewModel l'ha resolt el contenidor com si el
    /// pare l'ha fet amb <c>new</c>.
    /// </summary>
    public class ViewModelBase : ReactiveValidationObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();
    }
}

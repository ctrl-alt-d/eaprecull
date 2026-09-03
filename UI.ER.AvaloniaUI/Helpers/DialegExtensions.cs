using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using CommonInterfaces;
using ReactiveUI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Helpers
{
    /// <summary>
    /// Atén les <see cref="Interaction{TInput,TOutput}"/> dels ViewModels obrint la
    /// finestra corresponent. Concentra el bloc «resol la finestra, mostra-la modal
    /// sobre la meva, torna el resultat» que estava copiat 25 cops pel codi rere les
    /// vistes.
    /// </summary>
    public static class DialegExtensions
    {
        /// <summary>Diàleg que retorna un resultat (el DTO desat, la fila triada…).</summary>
        public static IDisposable RegistraDialeg<TWindow, TEntrada, TSortida>(
            this Visual owner,
            IWindowFactory windows,
            Interaction<TEntrada, TSortida?> interaccio)
            where TWindow : Window
            where TEntrada : ViewModelBase
            where TSortida : class
            => interaccio.RegisterHandler(async interaction =>
            {
                var dialog = windows.GetWith<TWindow>(interaction.Input);

                var result = await dialog.ShowDialog<TSortida?>(owner.GetOwnerWindow());
                interaction.SetOutput(result);
            });

        /// <summary>Diàleg que només es mostra i es tanca, sense retornar res.</summary>
        public static IDisposable RegistraDialeg<TWindow, TEntrada>(
            this Visual owner,
            IWindowFactory windows,
            Interaction<TEntrada, Unit> interaccio)
            where TWindow : Window
            where TEntrada : ViewModelBase
            => interaccio.RegisterHandler(async interaction =>
            {
                var dialog = windows.GetWith<TWindow>(interaction.Input);

                await dialog.ShowDialog(owner.GetOwnerWindow());
                interaction.SetOutput(Unit.Default);
            });

        /// <summary>
        /// Lookup: obre una llista en mode selecció i en torna l'element triat. Els 16
        /// blocs de les finestres d'<c>Alumne</c> i d'<c>Actuacio</c> es redueixen a una
        /// línia cadascun.
        /// </summary>
        /// <param name="setViewModel">
        /// Fàbrica, no instància: cada obertura del lookup vol una llista acabada de
        /// carregar. Desapareixerà a R1, quan la <see cref="IWindowFactory"/> sàpiga
        /// construir el ViewModel amb arguments de runtime.
        /// </param>
        public static IDisposable RegistraLookup<TSetWindow>(
            this Visual owner,
            IWindowFactory windows,
            Interaction<Unit, IIdEtiquetaDescripcio?> lookup,
            Func<ViewModelBase> setViewModel)
            where TSetWindow : Window
            => lookup.RegisterHandler(async interaction =>
            {
                var dialog = windows.GetWith<TSetWindow>(setViewModel());

                var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(owner.GetOwnerWindow());
                interaction.SetOutput(result);
            });
    }
}

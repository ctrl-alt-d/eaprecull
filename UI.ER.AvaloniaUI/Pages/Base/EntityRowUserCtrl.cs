using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages.Base
{
    /// <summary>
    /// Fila d'una llista: obre el diàleg d'edició quan el ViewModel ho demana i tanca la
    /// finestra retornant el DTO quan la fila se selecciona en mode lookup.
    /// </summary>
    /// <typeparam name="TResultat">
    /// Què retorna el diàleg d'edició. Normalment el mateix DTO — per a aquest cas hi ha
    /// la sobrecàrrega de quatre paràmetres.
    /// </typeparam>
    public abstract class EntityRowUserCtrl<TVm, TUpdateVm, TUpdateWindow, TResultat, TDto>
        : ReactiveUserControl<TVm>
        where TVm : class, IRowViewModel<TUpdateVm, TResultat, TDto>
        where TUpdateVm : ViewModelBase
        where TUpdateWindow : Window
        where TResultat : class
        where TDto : class
    {
        protected IWindowFactory Windows { get; }

        protected EntityRowUserCtrl(IWindowFactory windows)
        {
            Windows = windows;

            this.WhenActivated(Register);
        }

        /// <inheritdoc cref="EntityEditWindow{TVm,TDto}.Register"/>
        protected virtual void Register(CompositeDisposable d)
            => PerCadaViewModel(d, (vm, dd) =>
            {
                this.RegistraDialeg<TUpdateWindow, TUpdateVm, TResultat>(Windows, vm.ShowUpdateDialog)
                    .DisposeWith(dd);

                vm.SeleccionarCommand.Subscribe(this.GetOwnerWindow().Close).DisposeWith(dd);
            });

        /// <summary>
        /// Executa <paramref name="accio"/> cada cop que la vista rep un ViewModel no nul,
        /// i registra al <see cref="CompositeDisposable"/> de l'activació tant la
        /// subscripció exterior com les que <paramref name="accio"/> hi afegeixi.
        /// </summary>
        /// <remarks>
        /// El segon paràmetre no és decoratiu: sense ell les subscripcions imbricades
        /// (i els <c>RegisterHandler</c>, que també retornen <c>IDisposable</c>) queden
        /// vives en desactivar-se la vista i s'acumulen a cada reactivació.
        /// </remarks>
        protected void PerCadaViewModel(CompositeDisposable d, Action<TVm, CompositeDisposable> accio)
            => this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vm => accio(vm!, d))
                .DisposeWith(d);
    }

    /// <summary>
    /// Cas habitual: el diàleg d'edició retorna el mateix DTO que la fila representa.
    /// </summary>
    public abstract class EntityRowUserCtrl<TVm, TUpdateVm, TUpdateWindow, TDto>
        : EntityRowUserCtrl<TVm, TUpdateVm, TUpdateWindow, TDto, TDto>
        where TVm : class, IRowViewModel<TUpdateVm, TDto, TDto>
        where TUpdateVm : ViewModelBase
        where TUpdateWindow : Window
        where TDto : class
    {
        protected EntityRowUserCtrl(IWindowFactory windows) : base(windows) { }
    }
}

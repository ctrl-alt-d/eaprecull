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
    /// Llista d'una entitat: atén la petició d'alta del ViewModel obrint
    /// <typeparamref name="TCreateWindow"/> i li retorna el DTO creat.
    /// </summary>
    public abstract class EntitySetWindow<TVm, TCreateVm, TCreateWindow, TDto> : ReactiveWindow<TVm>
        where TVm : ViewModelBase, ISetViewModel<TCreateVm, TDto>
        where TCreateVm : ViewModelBase
        where TCreateWindow : Window
        where TDto : class
    {
        protected IWindowFactory Windows { get; }

        protected EntitySetWindow(IWindowFactory windows)
        {
            Windows = windows;

            this.WhenActivated(Register);
        }

        /// <inheritdoc cref="EntityEditWindow{TVm,TDto}.Register"/>
        protected virtual void Register(CompositeDisposable d)
            => PerCadaViewModel(d, (vm, dd) =>
                this.RegistraDialeg<TCreateWindow, TCreateVm, TDto>(Windows, vm.ShowDialog).DisposeWith(dd));

        /// <inheritdoc cref="EntityRowUserCtrl{TVm,TUpdateVm,TUpdateWindow,TResultat,TDto}.PerCadaViewModel"/>
        protected void PerCadaViewModel(CompositeDisposable d, Action<TVm, CompositeDisposable> accio)
            => this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vm => accio(vm!, d))
                .DisposeWith(d);
    }
}

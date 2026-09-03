using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages.Base
{
    /// <summary>
    /// Diàleg d'edició d'una entitat: es tanca sol, retornant el DTO, quan el
    /// <c>SubmitCommand</c> del ViewModel acaba bé.
    /// </summary>
    /// <remarks>
    /// Cobreix tant les altes com les modificacions: els vuit fitxers
    /// <c>{Centre,CursAcademic,Etapa,TipusActuacio}{Create,Update}Window.axaml.cs</c>
    /// eren idèntics byte a byte tret del tipus, i per això no hi ha dues classes base
    /// separades. Les finestres amb lookups o commands propis (<c>Alumne*</c>,
    /// <c>Actuacio*</c>) hereten igual i amplien <see cref="Register"/>.
    /// </remarks>
    public abstract class EntityEditWindow<TVm, TDto> : ReactiveWindow<TVm>
        where TVm : ViewModelBase, ISubmitViewModel<TDto>
        where TDto : class
    {
        protected EntityEditWindow()
        {
            this.WhenActivated(Register);
        }

        /// <summary>
        /// Punt d'extensió per a les subscripcions de la finestra. Qui el sobreescrigui
        /// ha de cridar <c>base.Register(d)</c> per conservar el tancament automàtic.
        /// </summary>
        protected virtual void Register(CompositeDisposable d)
            => PerCadaViewModel(d, (vm, dd) => vm.SubmitCommand.Subscribe(TancaSiDesat).DisposeWith(dd));

        /// <inheritdoc cref="EntityRowUserCtrl{TVm,TUpdateVm,TUpdateWindow,TResultat,TDto}.PerCadaViewModel"/>
        protected void PerCadaViewModel(CompositeDisposable d, Action<TVm, CompositeDisposable> accio)
            => this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vm => accio(vm!, d))
                .DisposeWith(d);

        /// <summary>
        /// Què es retorna a qui ha obert el diàleg. Per defecte el DTO desat;
        /// <see cref="ActuacioUpdateWindow"/> el reempaqueta perquè també pot esborrar.
        /// </summary>
        protected virtual object? ResultatDeTancament(TDto desat) => desat;

        private void TancaSiDesat(TDto? desat)
        {
            if (desat is not null)
                Close(ResultatDeTancament(desat));
        }
    }
}

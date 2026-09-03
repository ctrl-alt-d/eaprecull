using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioUpdateWindow : EntityEditWindow<ActuacioUpdateViewModel, Dtoo.Actuacio>
    {
        private readonly IWindowFactory _windows;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public ActuacioUpdateWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioUpdateWindow(IWindowFactory windows)
        {
            _windows = windows;

            InitializeComponent();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        /// <summary>
        /// A diferència de la resta de diàlegs d'edició, aquest també pot esborrar: qui
        /// l'obre necessita saber quina de les dues coses ha passat.
        /// </summary>
        protected override object? ResultatDeTancament(Dtoo.Actuacio desat)
            => Dtoo.EditDialogResult<Dtoo.Actuacio>.Updated(desat);

        protected override void Register(CompositeDisposable d)
        {
            // Tancar la finestra quan s'hagi desat.
            base.Register(d);

            PerCadaViewModel(d, (vm, dd) =>
            {
                // Tancar si s'ha esborrat.
                vm.DeleteCommand.Subscribe(TancaSiEsborrat).DisposeWith(dd);

                // Diàleg de confirmació per esborrar.
                this.RegistraConfirmacio(_windows, vm.ShowDeleteConfirmation,
                    "Esborrar actuació", "Sí, esborrar")
                    .DisposeWith(dd);

                // Lookups.
                this.RegistraLookup<AlumneSetWindow>(_windows, vm.ShowAlumneLookup)
                    .DisposeWith(dd);

                this.RegistraLookup<TipusActuacioSetWindow>(_windows, vm.ShowTipusActuacioLookup)
                    .DisposeWith(dd);

                this.RegistraLookup<CentreSetWindow>(_windows, vm.ShowCentreLookup)
                    .DisposeWith(dd);

                this.RegistraLookup<EtapaSetWindow>(_windows, vm.ShowEtapaAlMomentDeLactuacioLookup)
                    .DisposeWith(dd);

                this.RegistraLookup<CursAcademicSetWindow>(_windows, vm.ShowCursActuacioLookup)
                    .DisposeWith(dd);
            });
        }

        private void TancaSiEsborrat(OperationResult<Dtoo.Actuacio>? result)
        {
            // Si result és null, l'usuari ha cancel·lat.
            // Si té BrokenRules, hi ha hagut un error (mostrat al ViewModel).
            // Si Data no és null, s'ha esborrat correctament.
            if (result?.Data != null)
                Close(Dtoo.EditDialogResult<Dtoo.Actuacio>.Deleted(result.Data.Id));
        }
    }
}

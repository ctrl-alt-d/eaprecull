using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioCreateWindow : EntityEditWindow<ActuacioCreateViewModel, Dtoo.Actuacio>
    {
        private readonly IWindowFactory _windows;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public ActuacioCreateWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioCreateWindow(IWindowFactory windows)
        {
            _windows = windows;

            InitializeComponent();
            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void Register(CompositeDisposable d)
        {
            // Tancar la finestra quan s'hagi desat.
            base.Register(d);

            PerCadaViewModel(d, (vm, dd) =>
            {
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
    }
}

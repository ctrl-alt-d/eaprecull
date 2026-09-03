using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia.Markup.Xaml;
using CommonInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class AlumneRowUserCtrl
        : EntityRowUserCtrl<AlumneRowViewModel, AlumneUpdateViewModel, AlumneUpdateWindow, Dtoo.Alumne>
    {
        // El ListBox.ItemTemplate instancia aquest control des de l'AXAML, no pas
        // el contenidor: cal un constructor sense paràmetres que resolgui la factory.
        public AlumneRowUserCtrl() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public AlumneRowUserCtrl(IWindowFactory windows) : base(windows) => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void Register(CompositeDisposable d)
        {
            // Diàleg d'edició i selecció en mode lookup.
            base.Register(d);

            // Actuacions de l'alumne.
            PerCadaViewModel(d, (vm, dd) =>
                this.RegistraDialeg<ActuacioSetWindow, ActuacioSetViewModel, IIdEtiquetaDescripcio>(
                    Windows, vm.ShowActuacioSetDialog).DisposeWith(dd));

            // Expedient de l'alumne.
            PerCadaViewModel(d, (vm, dd) =>
                this.RegistraDialeg<AlumneInformeViewerWindow, AlumneInformeViewerViewModel>(
                    Windows, vm.ShowInformeViewerDialog).DisposeWith(dd));

            // L'informe generat: obrir la carpeta on ha anat a parar.
            PerCadaViewModel(d, (vm, dd) =>
                vm.GeneraInformeCommand.Subscribe(FileExplorer.Obre).DisposeWith(dd));
        }
    }
}

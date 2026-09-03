using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioRowUserCtrl
        : EntityRowUserCtrl<ActuacioRowViewModel, ActuacioUpdateViewModel, ActuacioUpdateWindow,
                            Dtoo.EditDialogResult<Dtoo.Actuacio>, Dtoo.Actuacio>
    {
        // El ListBox.ItemTemplate instancia aquest control des de l'AXAML, no pas
        // el contenidor: cal un constructor sense paràmetres que resolgui la factory.
        public ActuacioRowUserCtrl() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioRowUserCtrl(IWindowFactory windows) : base(windows) => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void Register(CompositeDisposable d)
        {
            // Diàleg d'edició i selecció en mode lookup.
            base.Register(d);

            // Expedient de l'alumne de l'actuació.
            PerCadaViewModel(d, (vm, dd) =>
                this.RegistraDialeg<AlumneInformeViewerWindow, AlumneInformeViewerViewModel>(
                    Windows, vm.ShowExpedientAlumneDialog).DisposeWith(dd));

            // Fitxa de l'alumne de l'actuació.
            PerCadaViewModel(d, (vm, dd) =>
                this.RegistraDialeg<AlumneUpdateWindow, AlumneUpdateViewModel, Dtoo.Alumne>(
                    Windows, vm.ShowEditarAlumneDialog).DisposeWith(dd));
        }
    }
}

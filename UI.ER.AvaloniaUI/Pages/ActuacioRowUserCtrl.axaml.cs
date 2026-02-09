using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using ReactiveUI.Avalonia;
using System;
using System.Reactive.Linq;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioRowUserCtrl : ReactiveUserControl<ActuacioRowViewModel>
    {
        public ActuacioRowUserCtrl()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterShowUpdateDialog(disposables);
                RegisterShowExpedientAlumneDialog(disposables);
                RegisterShowEditarAlumneDialog(disposables);
                RegisterCloseOnSelect(disposables);
            });


        }

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);
        private Window GetWindow()
            =>
            (Window)this.VisualRoot!;

        // -- Show Dialog --
        protected virtual void RegisterShowUpdateDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm != null)
                .Subscribe(vm => vm!.ShowUpdateDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new ActuacioUpdateWindow()
                    {
                        DataContext = interaction.Input
                    };

                    var result = await dialog.ShowDialog<Dtoo.EditDialogResult<Dtoo.Actuacio>?>(GetWindow());

                    interaction.SetOutput(result);
                }))
            );

        // -- Show expedient alumne
        protected virtual void RegisterShowExpedientAlumneDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm != null)
                .Subscribe(vm => vm!.ShowExpedientAlumneDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new AlumneInformeViewerWindow()
                    {
                        DataContext = interaction.Input
                    };
                    await dialog.ShowDialog(GetWindow());
                    interaction.SetOutput(System.Reactive.Unit.Default);
                }))
            );

        // -- Show editar alumne
        protected virtual void RegisterShowEditarAlumneDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm != null)
                .Subscribe(vm => vm!.ShowEditarAlumneDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new AlumneUpdateWindow()
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<Dtoo.Alumne?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        // -- Select Row
        private void RegisterCloseOnSelect(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm != null)
                .Subscribe(vm => vm!.SeleccionarCommand.Subscribe(GetWindow().Close))
            );


    }
}
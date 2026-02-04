using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using ReactiveUI.Avalonia;
using System;
using System.Reactive;
using CommonInterfaces;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class AlumneRowUserCtrl : ReactiveUserControl<AlumneRowViewModel>
    {
        public AlumneRowUserCtrl()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterShowUpdateDialog(disposables);
                RegisterShowActuacioDialog(disposables);
                RegisterCloseOnSelect(disposables);
                RegisterInformeActuacions(disposables);
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
                .Subscribe(vm => vm!.ShowUpdateDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new AlumneUpdateWindow()
                    {
                        DataContext = interaction.Input
                    };

                    var result = await dialog.ShowDialog<dtoo.Alumne?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        // -- Show actuacions
        protected virtual void RegisterShowActuacioDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowActuacioSetDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new ActuacioSetWindow()
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        // -- Select Row
        private void RegisterCloseOnSelect(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.SeleccionarCommand.Subscribe(GetWindow().Close))
            );


        // -- Infoem Actuacions
        private void RegisterInformeActuacions(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.GeneraInformeCommand.Subscribe(ObraFileExplorer))
            );

        private void ObraFileExplorer(dtoo.SaveResult? saveResult)
        {
            if (saveResult == null) return;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = saveResult.FolderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
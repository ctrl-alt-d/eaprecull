using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    /// <summary>
    /// Expedient d'un alumne, amb exportació a Word.
    /// </summary>
    public partial class AlumneInformeViewerWindow : ReactiveWindow<AlumneInformeViewerViewModel>
    {
        public AlumneInformeViewerWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
                this.WhenAnyValue(x => x.ViewModel)
                    .Where(vm => vm is not null)
                    .Subscribe(vm =>
                    {
                        vm!.CloseCommand.Subscribe(_ => Close()).DisposeWith(d);
                        vm.ExportarWordCommand.Subscribe(FileExplorer.Obre).DisposeWith(d);

                        // La càrrega inicial anava a l'esdeveniment Opened amb un
                        // `async void`: una excepció de LoadData no tenia on anar a
                        // parar. Executada com a command, els errors surten per
                        // ThrownExceptions com els de la resta de l'aplicació.
                        vm.LoadDataCommand.Execute().Subscribe().DisposeWith(d);
                    })
                    .DisposeWith(d));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

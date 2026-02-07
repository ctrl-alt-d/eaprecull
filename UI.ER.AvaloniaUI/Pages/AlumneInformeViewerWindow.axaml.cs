using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UI.ER.ViewModels.ViewModels;
using Dtoo = DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class AlumneInformeViewerWindow : Window
    {
        public AlumneInformeViewerWindow()
        {
            InitializeComponent();

            // Quan s'obre la finestra, carregar les dades
            this.Opened += async (s, e) =>
            {
                if (DataContext is AlumneInformeViewerViewModel vm)
                {
                    await vm.LoadDataCommand.Execute();
                }
            };

            // Subscripcions als commands
            this.WhenAnyValue(x => x.DataContext)
                .Where(dc => dc != null)
                .Subscribe(dc =>
                {
                    if (dc is AlumneInformeViewerViewModel vm)
                    {
                        vm.CloseCommand.Subscribe(_ => Close());
                        vm.ExportarWordCommand.Subscribe(ObraFileExplorer);
                    }
                });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ObraFileExplorer(Dtoo.SaveResult? saveResult)
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

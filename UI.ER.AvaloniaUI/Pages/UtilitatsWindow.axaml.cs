using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using Dtoo = DTO.o.DTOs;
using System.Reactive.Linq;
using System.Linq;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class UtilitatsWindow : ReactiveWindow<UtilitatsViewModel>
    {
        public UtilitatsWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterPivot(disposables);
            });
        }

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);

        private Window GetWindow()
            =>
            (Window)this.VisualRoot!;

        private void RegisterPivot(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.GeneraPivotCommand.Subscribe(ObraFileExplorer))
            );

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
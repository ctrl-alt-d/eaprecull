using System;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive.Linq;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.ViewModels.ViewModels;

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

        private void RegisterPivot(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vm => vm!.GeneraPivotCommand.Subscribe(FileExplorer.Obre))
            );
    }
}

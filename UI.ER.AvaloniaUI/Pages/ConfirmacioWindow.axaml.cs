using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    /// <summary>
    /// «Segur que…?». Substitueix <c>Helpers/ConfirmationDialog</c>, que construïa la
    /// UI en C# amb colors literals i deia sempre «Sí, esborrar» fos quin fos el
    /// missatge (R6).
    /// </summary>
    public partial class ConfirmacioWindow : ReactiveWindow<ConfirmacioViewModel>
    {
        public ConfirmacioWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
                this.WhenAnyValue(x => x.ViewModel)
                    .Where(vm => vm is not null)
                    .Subscribe(vm =>
                    {
                        vm!.ConfirmaCommand.Subscribe(Close).DisposeWith(d);
                        vm.CancelaCommand.Subscribe(Close).DisposeWith(d);
                    })
                    .DisposeWith(d));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void Close(bool confirmat) => Close((object)confirmat);
    }
}

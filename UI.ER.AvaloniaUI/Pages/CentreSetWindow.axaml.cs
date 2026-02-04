using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using dtoo = DTO.o.DTOs;
using System.Reactive.Linq;
using System.Linq;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class CentreSetWindow : ReactiveWindow<CentreSetViewModel>
    {
        public CentreSetWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterShowCreateDialog(disposables);
            });
        }

        private void RegisterShowCreateDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new CentreCreateWindow()
                    {
                        DataContext = interaction.Input
                    };

                    var result = await dialog.ShowDialog<dtoo.Centre?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);

        private Window GetWindow()
            =>
            (Window)this.VisualRoot!;
    }
}
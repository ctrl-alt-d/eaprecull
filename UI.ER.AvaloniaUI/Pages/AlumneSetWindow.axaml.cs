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
    public class AlumneSetWindow : ReactiveWindow<AlumneSetViewModel>
    {
        public AlumneSetWindow()
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
                .Subscribe(vm => vm.ShowDialog.RegisterHandler(DoShowCreateDialog))
            );

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);

        private Window GetWindow()
            =>
            (Window)this.VisualRoot;

        private async Task DoShowCreateDialog(InteractionContext<AlumneCreateViewModel, dtoo.Alumne?> interaction)
        {
            var dialog = new AlumneCreateWindow()
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<dtoo.Alumne?>(GetWindow());
            interaction.SetOutput(result);

        }
    }
}
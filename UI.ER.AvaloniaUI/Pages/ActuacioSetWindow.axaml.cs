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
    public class ActuacioSetWindow : ReactiveWindow<ActuacioSetViewModel> {
        public ActuacioSetWindow() {
            InitializeComponent();                        
            this.WhenActivated(disposables => {
                RegisterShowCreateDialog(disposables);
            });
        }

        private void RegisterShowCreateDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm => vm.ShowDialog.RegisterHandler(DoShowCreateDialog))
            );

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);      
        
        private Window GetWindow()
            =>
            (Window)this.VisualRoot;

        private async Task DoShowCreateDialog(InteractionContext<ActuacioCreateViewModel, dtoo.Actuacio?> interaction)
        {
            var dialog = new ActuacioCreateWindow()
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<dtoo.Actuacio?>(GetWindow());
            interaction.SetOutput(result);
            
        }
    }
}
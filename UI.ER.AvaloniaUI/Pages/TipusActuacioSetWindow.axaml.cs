using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;
using dtoo = DTO.o.DTOs;
using System.Reactive.Linq;
using DynamicData.Binding;
using DynamicData;
using System.Linq;

namespace UI.ER.AvaloniaUI.Pages {
    public class TipusActuacioSetWindow : ReactiveWindow<TipusActuacioSetViewModel> {
        public TipusActuacioSetWindow() {
            InitializeComponent();
                        
            this.WhenActivated(d => {

                // Crear nou item
                d(ViewModel!.ShowDialog.RegisterHandler(CreateShowDialogAsync));

                // Tancar la finestra si seleccionen item
                d(ViewModel
                    .WhenAnyValue(x => x.SelectedItem)
                    .Where(s => s != null)
                    .Select(x => x)
                    .Subscribe(x=>Close(x)));
            });
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);            
        }

        private async Task CreateShowDialogAsync(InteractionContext<TipusActuacioCreateViewModel, dtoo.TipusActuacio?> interaction)
        {
            var dialog = new TipusActuacioCreateWindow()
            {
                DataContext = interaction.Input
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<dtoo.TipusActuacio?>(window);
            interaction.SetOutput(result);
            
        }
    }
}
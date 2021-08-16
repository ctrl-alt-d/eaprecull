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
    public class CentreSetWindow : ReactiveWindow<CentreSetViewModel> {
        public CentreSetWindow() {
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

        private async Task CreateShowDialogAsync(InteractionContext<CentreCreateViewModel, dtoo.Centre?> interaction)
        {
            var dialog = new CentreCreateWindow()
            {
                DataContext = interaction.Input
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<dtoo.Centre?>(window);
            interaction.SetOutput(result);
            
        }
    }
}
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

namespace UI.ER.AvaloniaUI.Pages {
    public class CentreSetWindow : ReactiveWindow<CentreSetViewModel> {
        public CentreSetWindow() {
            InitializeComponent();
                        
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(CreateShowDialogAsync)));
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
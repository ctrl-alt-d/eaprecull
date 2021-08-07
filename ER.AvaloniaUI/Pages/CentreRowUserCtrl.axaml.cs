using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using ER.AvaloniaUI.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using Avalonia.ReactiveUI;

namespace ER.AvaloniaUI.Pages
{
    public class CentreRowUserCtrl : ReactiveUserControl<CentreRowViewModel> {
        public CentreRowUserCtrl( ) {
            InitializeComponent();            
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(CreateShowDialogAsync)));
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task CreateShowDialogAsync(InteractionContext<CentreCreateViewModel, OperationResult<dtoo.Centre>?> interaction)
        {
            var dialog = new CentreCreateWindow();
            dialog.DataContext = interaction.Input;

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await dialog.ShowDialog<OperationResult<dtoo.Centre>?>(desktop.MainWindow);
                interaction.SetOutput(result);
            }
        }

    }
}
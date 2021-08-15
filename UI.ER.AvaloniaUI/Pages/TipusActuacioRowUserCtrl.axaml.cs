using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using Avalonia.ReactiveUI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.AvaloniaUI.Views;

namespace UI.ER.AvaloniaUI.Pages
{
    public class TipusActuacioRowUserCtrl : ReactiveUserControl<TipusActuacioRowViewModel>
    {
        public TipusActuacioRowUserCtrl()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(UpdateShowDialogAsync)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task UpdateShowDialogAsync(InteractionContext<TipusActuacioUpdateViewModel, dtoo.TipusActuacio?> interaction)
        {
            var dialog = new TipusActuacioUpdateWindow()
            {
                DataContext = interaction.Input
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<dtoo.TipusActuacio?>(window);
            interaction.SetOutput(result);
            
        }
    }
}
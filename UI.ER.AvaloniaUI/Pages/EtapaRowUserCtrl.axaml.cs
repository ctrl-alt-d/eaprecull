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
    public class EtapaRowUserCtrl : ReactiveUserControl<EtapaRowViewModel>
    {
        public EtapaRowUserCtrl()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(UpdateShowDialogAsync)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task UpdateShowDialogAsync(InteractionContext<EtapaUpdateViewModel, dtoo.Etapa?> interaction)
        {
            var dialog = new EtapaUpdateWindow()
            {
                DataContext = interaction.Input
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<dtoo.Etapa?>(window);
            interaction.SetOutput(result);
            
        }
    }
}
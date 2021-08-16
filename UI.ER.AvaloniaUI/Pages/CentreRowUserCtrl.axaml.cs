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
using Avalonia.LogicalTree;

namespace UI.ER.AvaloniaUI.Pages
{
    public class CentreRowUserCtrl : ReactiveUserControl<CentreRowViewModel>
    {
        public CentreRowUserCtrl()
        {
            InitializeComponent();
            // this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(UpdateShowDialogAsync)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);            
        }

        protected override void OnDataContextEndUpdate()
        {
            base.OnDataContextBeginUpdate();
            ViewModel?.ShowDialog.RegisterHandler(UpdateShowDialogAsync);
            
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        private async Task UpdateShowDialogAsync(InteractionContext<CentreUpdateViewModel, dtoo.Centre?> interaction)
        {
            var dialog = new CentreUpdateWindow()
            {
                DataContext = interaction.Input
            };

            var window = (Window)this.VisualRoot;

            var result = await dialog.ShowDialog<dtoo.Centre?>(window);

            interaction.SetOutput(result);
            ( this.Parent as ListBoxItem)!.Focus();
        }
    }
}
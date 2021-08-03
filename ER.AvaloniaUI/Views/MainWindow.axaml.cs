using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ER.AvaloniaUI.ViewModels;
using ReactiveUI;

namespace ER.AvaloniaUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));        
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowDialogAsync(InteractionContext<CentresViewModel, CentreViewModel?> interaction)
        {
            var dialog = new CentresWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<CentreViewModel?>(this);
            interaction.SetOutput(result);
        }
    }
}
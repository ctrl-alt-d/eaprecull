using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ER.AvaloniaUI.Views
{
    public partial class CentresWindow : Window
    {
        public CentresWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
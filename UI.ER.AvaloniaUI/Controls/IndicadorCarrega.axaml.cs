using Avalonia;
using Avalonia.Controls;

namespace UI.ER.AvaloniaUI.Controls
{
    /// <summary>
    /// «Carregant dades…» amb la icona giratòria. Els sis llistats en tenien una còpia
    /// idèntica de vuit línies cadascun (R6).
    /// </summary>
    public partial class IndicadorCarrega : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<IndicadorCarrega, string>(
                nameof(Text), "Carregant dades ...");

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public IndicadorCarrega()
        {
            InitializeComponent();
        }
    }
}

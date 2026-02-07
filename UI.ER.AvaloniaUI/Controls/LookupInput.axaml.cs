using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace UI.ER.AvaloniaUI.Controls
{
    public partial class LookupInput : UserControl
    {
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<LookupInput, string>(nameof(Label), "Selecció");

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<LookupInput, string>(nameof(Text), string.Empty);

        public static readonly StyledProperty<ICommand?> LookupCommandProperty =
            AvaloniaProperty.Register<LookupInput, ICommand?>(nameof(LookupCommand));

        public static readonly StyledProperty<ICommand?> ClearCommandProperty =
            AvaloniaProperty.Register<LookupInput, ICommand?>(nameof(ClearCommand));

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand? LookupCommand
        {
            get => GetValue(LookupCommandProperty);
            set => SetValue(LookupCommandProperty, value);
        }

        public ICommand? ClearCommand
        {
            get => GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        public LookupInput()
        {
            InitializeComponent();
        }
    }
}

using Avalonia.Markup.Xaml;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class TipusActuacioCreateWindow : EntityEditWindow<TipusActuacioCreateViewModel, Dtoo.TipusActuacio>
    {
        public TipusActuacioCreateWindow() => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

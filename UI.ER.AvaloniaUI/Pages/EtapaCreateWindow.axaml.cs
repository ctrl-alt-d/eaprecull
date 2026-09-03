using Avalonia.Markup.Xaml;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class EtapaCreateWindow : EntityEditWindow<EtapaCreateViewModel, Dtoo.Etapa>
    {
        public EtapaCreateWindow() => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

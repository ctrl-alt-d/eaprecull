using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class EtapaRowUserCtrl
        : EntityRowUserCtrl<EtapaRowViewModel, EtapaUpdateViewModel, EtapaUpdateWindow, Dtoo.Etapa>
    {
        // El ListBox.ItemTemplate instancia aquest control des de l'AXAML, no pas
        // el contenidor: cal un constructor sense paràmetres que resolgui la factory.
        public EtapaRowUserCtrl() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public EtapaRowUserCtrl(IWindowFactory windows) : base(windows) => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

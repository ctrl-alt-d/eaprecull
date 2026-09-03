using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class EtapaSetWindow
        : EntitySetWindow<EtapaSetViewModel, EtapaCreateViewModel, EtapaCreateWindow, Dtoo.Etapa>
    {
        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen Windows a punt.
        public EtapaSetWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public EtapaSetWindow(IWindowFactory windows) : base(windows) => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioSetWindow
        : EntitySetWindow<ActuacioSetViewModel, ActuacioCreateViewModel, ActuacioCreateWindow, Dtoo.Actuacio>
    {
        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen Windows a punt.
        public ActuacioSetWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioSetWindow(IWindowFactory windows) : base(windows) => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Dtoo = DTO.o.DTOs;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;
using System;
using System.Reactive.Linq;
using DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Reactive;
using Avalonia;
using Avalonia.Input;
using UI.ER.AvaloniaUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioCreateWindow : ReactiveWindow<ActuacioCreateViewModel>
    {
        public OperationResult<Dtoo.Actuacio> Result { get; set; } = default!;
        private readonly IWindowFactory _windows;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public ActuacioCreateWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioCreateWindow(IWindowFactory windows)
        {
            _windows = windows;

            this.InitializeComponent();
            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));

            this.WhenActivated(d =>
            {

                // Tancar finestre
                d(
                    ViewModel!
                    .SubmitCommand
                    .Subscribe(CloseIfSaved)
                );

                // Lookups
                d(ViewModel!.ShowAlumneLookup.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<AlumneSetWindow>(new AlumneSetViewModel(modeLookup: true));

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowTipusActuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<TipusActuacioSetWindow>(new TipusActuacioSetViewModel(modeLookup: true));

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowCentreLookup.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<CentreSetWindow>(new CentreSetViewModel(modeLookup: true));

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowEtapaAlMomentDeLactuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<EtapaSetWindow>(new EtapaSetViewModel(modeLookup: true));

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowCursActuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<CursAcademicSetWindow>(new CursAcademicSetViewModel(modeLookup: true));

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
            });
        }

        private void CloseIfSaved(Actuacio? obj)
        {
            if (obj != null)
                Close(obj);
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    }
}

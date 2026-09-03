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
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioUpdateWindow : ReactiveWindow<ActuacioUpdateViewModel>
    {
        private readonly IWindowFactory _windows;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public ActuacioUpdateWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public ActuacioUpdateWindow(IWindowFactory windows)
        {
            _windows = windows;

            this.InitializeComponent();

            this.WhenActivated(d =>
            {

                // Tancar finestre
                d(
                    ViewModel!
                    .SubmitCommand
                    .Subscribe(CloseIfSaved)
                );

                // Tancar si s'ha esborrat
                d(
                    ViewModel!
                    .DeleteCommand
                    .Subscribe(CloseIfDeleted)
                );

                // Diàleg de confirmació per esborrar
                d(ViewModel!.ShowDeleteConfirmation.RegisterHandler(async interaction =>
                {
                    var window = (Window)this.VisualRoot!;
                    var result = await ConfirmationDialog.Show(window, interaction.Input, "Esborrar actuació");
                    interaction.SetOutput(result);
                }));

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
                Close(EditDialogResult<Actuacio>.Updated(obj));
        }

        private void CloseIfDeleted(OperationResult<Actuacio>? result)
        {
            // Si result és null, l'usuari ha cancel·lat
            // Si té BrokenRules, hi ha hagut un error (mostrat al ViewModel)
            // Si Data no és null, s'ha esborrat correctament
            if (result?.Data != null)
                Close(EditDialogResult<Actuacio>.Deleted(result.Data.Id));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    }
}

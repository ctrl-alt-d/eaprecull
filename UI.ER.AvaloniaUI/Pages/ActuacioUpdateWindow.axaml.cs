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

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class ActuacioUpdateWindow : ReactiveWindow<ActuacioUpdateViewModel>
    {
        public ActuacioUpdateWindow()
        {
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
                    var dialog = new AlumneSetWindow()
                    {
                        DataContext = new AlumneSetViewModel(modeLookup: true)
                    };

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowTipusActuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = new TipusActuacioSetWindow()
                    {
                        DataContext = new TipusActuacioSetViewModel(modeLookup: true)
                    };

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowCentreLookup.RegisterHandler(async interaction =>
                {
                    var dialog = new CentreSetWindow()
                    {
                        DataContext = new CentreSetViewModel(modeLookup: true)
                    };

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowEtapaAlMomentDeLactuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = new EtapaSetWindow()
                    {
                        DataContext = new EtapaSetViewModel(modeLookup: true)
                    };

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowCursActuacioLookup.RegisterHandler(async interaction =>
                {
                    var dialog = new CursAcademicSetWindow()
                    {
                        DataContext = new CursAcademicSetViewModel(modeLookup: true)
                    };

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

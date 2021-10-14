using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using dtoo = DTO.o.DTOs;
using ReactiveUI;
using Avalonia.ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using System;
using System.Reactive.Linq;
using DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.Reactive;

namespace UI.ER.AvaloniaUI.Pages
{
    public class ActuacioUpdateWindow : ReactiveWindow<ActuacioUpdateViewModel>
    {
        public OperationResult<dtoo.Actuacio> Result { get; set; } = default!;
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

                // Lookups
                d(ViewModel!.ShowAlumneLookup.RegisterHandler(AlumneLookupShowDialogAsync));
                d(ViewModel!.ShowTipusActuacioLookup.RegisterHandler(TipusActuacioLookupShowDialogAsync));
                d(ViewModel!.ShowCentreLookup.RegisterHandler(CentreLookupShowDialogAsync));
                d(ViewModel!.ShowEtapaAlMomentDeLactuacioLookup.RegisterHandler(EtapaAlMomentDeLactuacioLookupShowDialogAsync));
                d(ViewModel!.ShowCursActuacioLookup.RegisterHandler(CursActuacioLookupShowDialogAsync));
            });
        }

        private void CloseIfSaved(Actuacio? obj)
        {
            if (obj != null)
                Close(obj);
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        //
        private async Task AlumneLookupShowDialogAsync(InteractionContext<Unit, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new AlumneSetWindow()
            {
                DataContext = new AlumneSetViewModel(modeLookup: true)
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
            interaction.SetOutput(result);
        }


        //
        private async Task TipusActuacioLookupShowDialogAsync(InteractionContext<Unit, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new TipusActuacioSetWindow()
            {
                DataContext = new TipusActuacioSetViewModel(modeLookup: true)
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
            interaction.SetOutput(result);
        }
        //
        private async Task CentreLookupShowDialogAsync(InteractionContext<Unit, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new CentreSetWindow()
            {
                DataContext = new CentreSetViewModel(modeLookup: true)
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
            interaction.SetOutput(result);
        }


        private async Task EtapaAlMomentDeLactuacioLookupShowDialogAsync(InteractionContext<Unit, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new EtapaSetWindow()
            {
                DataContext = new EtapaSetViewModel(modeLookup: true)
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
            interaction.SetOutput(result);
        }
        private async Task CursActuacioLookupShowDialogAsync(InteractionContext<Unit, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new CursAcademicSetWindow()
            {
                DataContext = new CursAcademicSetViewModel(modeLookup: true)
            };

            var window = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
            interaction.SetOutput(result);
        }

    }
}

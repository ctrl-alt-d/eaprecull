using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Dtoo = DTO.o.DTOs;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;
using System;
using System.Reactive.Linq;
using DTO.o.DTOs;
using System.Threading.Tasks;
using System.Reactive;
using CommonInterfaces;
using Avalonia.Controls;


namespace UI.ER.AvaloniaUI.Pages
{
    public partial class AlumneUpdateWindow : ReactiveWindow<AlumneUpdateViewModel>
    {
        public OperationResult<Dtoo.Alumne> Result { get; set; } = default!;
        public AlumneUpdateWindow()
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
                d(ViewModel!.ShowEtapaActualLookup.RegisterHandler(async interaction =>
                {
                    var dialog = new EtapaSetWindow()
                    {
                        DataContext = new EtapaSetViewModel(modeLookup: true)
                    };

                    var window = (Window)this.VisualRoot!;
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(window);
                    interaction.SetOutput(result);
                }));
                d(ViewModel!.ShowCursDarreraActualitacioDadesLookup.RegisterHandler(async interaction =>
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
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        // -- Close if saved --
        private void CloseIfSaved(Alumne? obj)
        {
            if (obj != null)
                Close(obj);
        }

    }
}

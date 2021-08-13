using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Material.Dialog.Interfaces;
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
    public class AlumneCreateWindow : ReactiveWindow<AlumneCreateViewModel>
    { 
        public OperationResult<dtoo.Alumne> Result { get; set; } = default!;
        public AlumneCreateWindow()
        {
            this.DataContext = new AlumneCreateViewModel(); // ToDo: issue20. Això anirà al dataset.
            this.InitializeComponent();
            
            this.WhenActivated(d => {
                d(
                    ViewModel
                    .WhenAnyValue(x => x.SuccessfullySaved)
                    .CombineLatest(ViewModel!.SubmitCommand, (saved, obj) => (saved, obj))
                    .Where(s => s.saved)
                    .Select(s => s.obj)
                    .Subscribe(Close)
                );

                d(ViewModel!.ShowCentreLookup.RegisterHandler(CentreLookupShowDialogAsync));
            });
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

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
    }
}

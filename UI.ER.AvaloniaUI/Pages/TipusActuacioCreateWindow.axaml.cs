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

namespace UI.ER.AvaloniaUI.Pages
{
    public class TipusActuacioCreateWindow : ReactiveWindow<TipusActuacioCreateViewModel>
    { 
        public OperationResult<dtoo.TipusActuacio> Result { get; set; } = default!;
        public TipusActuacioCreateWindow()
        {
            this.InitializeComponent();
            this.WhenActivated(d => {

                // Tancar la finestra.
                d(
                    ViewModel!
                    .SubmitCommand
                    .Subscribe(CloseIfSaved));
                    
            });                    
        }

        private void CloseIfSaved(TipusActuacio? obj)
        {
            if (ViewModel!.SuccessfullySaved)
                Close(obj);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

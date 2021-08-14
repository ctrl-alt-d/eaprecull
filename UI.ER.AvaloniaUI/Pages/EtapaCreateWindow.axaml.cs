using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using dtoo = DTO.o.DTOs;
using ReactiveUI;
using Avalonia.ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using System;
using System.Reactive.Linq;
using DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.Pages
{
    public class EtapaCreateWindow : ReactiveWindow<EtapaCreateViewModel>
    { 
        public OperationResult<dtoo.Etapa> Result { get; set; } = default!;
        public EtapaCreateWindow()
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

        private void CloseIfSaved(Etapa? obj)
        {
            if (ViewModel!.SuccessfullySaved)
                Close(obj);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

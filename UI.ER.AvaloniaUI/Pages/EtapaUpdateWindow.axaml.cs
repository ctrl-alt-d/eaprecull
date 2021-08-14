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
    public class EtapaUpdateWindow : ReactiveWindow<EtapaUpdateViewModel>
    {
        public OperationResult<dtoo.Etapa> Result { get; set; } = default!;
        public EtapaUpdateWindow()
        {
            this.InitializeComponent();
            
            this.WhenActivated(d => {

                // https://stackoverflow.com/questions/68747035/subscribe-to-close-but-close-only-if-item-was-saved
                d(
                    ViewModel!
                    .SubmitCommand
                    .Subscribe(CloseIfSaved)
                );
                
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

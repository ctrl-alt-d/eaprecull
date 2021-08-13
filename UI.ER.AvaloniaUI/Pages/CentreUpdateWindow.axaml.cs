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
    public class CentreUpdateWindow : ReactiveWindow<CentreUpdateViewModel>
    {
        public OperationResult<dtoo.Centre> Result { get; set; } = default!;
        public CentreUpdateWindow()
        {
            this.InitializeComponent();
            
            this.WhenActivated(d => {

                // https://stackoverflow.com/questions/68747035/subscribe-to-close-but-close-only-if-item-was-saved
                d(
                    ViewModel
                    .WhenAnyValue(x => x.SuccessfullySaved)
                    .CombineLatest(ViewModel!.SubmitCommand, (saved, obj) => (saved, obj))
                    .Where(s => s.saved)
                    .Select(s => s.obj)
                    .Subscribe(Close)
                );
                
            });
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

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
    public class CentreCreateWindow : ReactiveWindow<CentreCreateViewModel>
    { 
        public OperationResult<dtoo.Centre> Result { get; set; } = default!;
        public CentreCreateWindow()
        {
            this.InitializeComponent();
            this.WhenActivated(d => {

                // Tancar la finestra.
                d(
                    ViewModel
                    .WhenAnyValue(x => x.SuccessfullySaved)
                    .CombineLatest(ViewModel!.SubmitCommand, (saved, obj) => (saved, obj))
                    .Where(s => s.saved)
                    .Select(s => s.obj)
                    .Subscribe(Close));
                    
            });                    
        }


        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

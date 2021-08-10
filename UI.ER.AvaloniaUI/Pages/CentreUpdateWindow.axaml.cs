using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Material.Dialog.Interfaces;
using dtoo = DTO.o.DTOs;
using ReactiveUI;
using Avalonia.ReactiveUI;
using UI.ER.AvaloniaUI.ViewModels;
using System;

namespace UI.ER.AvaloniaUI.Pages
{
    public class CentreUpdateWindow : ReactiveWindow<CentreUpdateViewModel>, IDialogWindowResult<OperationResult<dtoo.Centre>>
    { 
        public OperationResult<dtoo.Centre> Result { get; set; } = default!;
        public CentreUpdateWindow()
        {
            this.InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.SubmitCommand.Subscribe(Close)));
        }        
        public OperationResult<dtoo.Centre> GetResult() => Result;
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

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
    public class CentreUpdateWindow : ReactiveWindow<CentreUpdateViewModel>, IDialogWindowResult<OperationResult<dtoo.Centre>>
    { 
        public OperationResult<dtoo.Centre> Result { get; set; } = default!;
        public CentreUpdateWindow()
        {
            this.InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.SubmitCommand.Subscribe(SortirSiCal)));
        }

        private void SortirSiCal(Centre? obj)
        {
            if (ViewModel!.Sortir)
                Close(obj);
        }

        public OperationResult<dtoo.Centre> GetResult() => Result;
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

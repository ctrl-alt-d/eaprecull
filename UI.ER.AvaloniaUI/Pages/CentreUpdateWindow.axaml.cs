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
            this.WhenActivated(d => d(ViewModel!.SubmitCommand.Subscribe(CloseIfSaved)));
        }

        private void CloseIfSaved(Centre? obj)
        {
            if (ViewModel!.SuccessfullySaved)
                Close(obj);
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

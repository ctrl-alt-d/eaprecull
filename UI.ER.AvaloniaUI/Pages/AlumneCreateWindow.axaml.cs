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
    public class AlumneCreateWindow : ReactiveWindow<AlumneCreateViewModel>
    { 
        public OperationResult<dtoo.Alumne> Result { get; set; } = default!;
        public AlumneCreateWindow()
        {
            this.DataContext = new AlumneCreateViewModel(); // ToDo: issue20. Això anirà al dataset.
            this.InitializeComponent();
            
            this.WhenActivated(d =>
                d(
                    ViewModel
                    .WhenAnyValue(x => x.SuccessfullySaved)
                    .CombineLatest(ViewModel!.SubmitCommand, (saved, obj) => (saved, obj))
                    .Where(s => s.saved)
                    .Select(s => s.obj)
                    .Subscribe(Close)
                ));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}

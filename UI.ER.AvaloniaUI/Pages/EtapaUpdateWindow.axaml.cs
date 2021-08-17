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
    public class EtapaUpdateWindow : ReactiveWindow<EtapaUpdateViewModel>
    {
        public OperationResult<dtoo.Etapa> Result { get; set; } = default!;
        public EtapaUpdateWindow()
        {
            this.InitializeComponent();
            
            this.WhenActivated(disposables => { 
                RegisterCloseIfSaved(disposables); 
            });
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        // -- Close if saved --
        protected virtual void RegisterCloseIfSaved(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm=>
                    vm.SubmitCommand.Subscribe(CloseIfSaved)
                )
            );

        private void CloseIfSaved(Etapa? obj)
        {
            if (obj != null)
                Close(obj);
        }

    }
}

using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Dtoo = DTO.o.DTOs;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;
using System;
using System.Reactive.Linq;
using DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class CursAcademicUpdateWindow : ReactiveWindow<CursAcademicUpdateViewModel>
    {
        public OperationResult<Dtoo.CursAcademic> Result { get; set; } = default!;
        public CursAcademicUpdateWindow()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterCloseIfSaved(disposables);
            });
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        // -- Close if saved --
        protected virtual void RegisterCloseIfSaved(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm =>
                    vm!.SubmitCommand.Subscribe(CloseIfSaved)
                )
            );

        private void CloseIfSaved(CursAcademic? obj)
        {
            if (obj != null)
                Close(obj);
        }

    }
}

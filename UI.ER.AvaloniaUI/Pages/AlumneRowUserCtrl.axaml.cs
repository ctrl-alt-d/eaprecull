using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using Avalonia.ReactiveUI;
using System;
using System.Reactive;
using CommonInterfaces;

namespace UI.ER.AvaloniaUI.Pages
{
    public class AlumneRowUserCtrl : ReactiveUserControl<AlumneRowViewModel>
    {
        public AlumneRowUserCtrl()
        {
            InitializeComponent();

            this.WhenActivated(disposables => { 
                RegisterShowUpdateDialog(disposables);
                RegisterShowActuacioDialog(disposables); 
                RegisterCloseOnSelect(disposables); 
            });


        }

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);
        private Window GetWindow() 
            =>
            (Window)this.VisualRoot;

        // -- Show Dialog --
        protected virtual void RegisterShowUpdateDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm=>vm.ShowUpdateDialog.RegisterHandler(DoShowUpdateDialog))
            );        
        protected virtual async Task DoShowUpdateDialog(InteractionContext<AlumneUpdateViewModel, dtoo.Alumne?> interaction)
        {
            var dialog = new AlumneUpdateWindow()
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<dtoo.Alumne?>(GetWindow());

            interaction.SetOutput(result);        
        }

        // -- Show actuacions
        protected virtual void RegisterShowActuacioDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm=>vm.ShowActuacioSetDialog.RegisterHandler(DoShowActuacioLookup))
            );        
        protected virtual async Task DoShowActuacioLookup(InteractionContext<ActuacioSetViewModel, IIdEtiquetaDescripcio?> interaction)
        {
            var dialog = new ActuacioSetWindow()
            {
                DataContext =interaction.Input
            };
            var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(GetWindow());
            interaction.SetOutput(result);        
        }

        // -- Select Row
        private void RegisterCloseOnSelect(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm=>vm.SeleccionarCommand.Subscribe(GetWindow().Close))
            );


    }
}
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using Avalonia.ReactiveUI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.AvaloniaUI.Views;
using Avalonia.LogicalTree;
using System.Collections.Generic;
using System;
using System.Reactive;

namespace UI.ER.AvaloniaUI.Pages
{
    public class CentreRowUserCtrl : ReactiveUserControl<CentreRowViewModel>
    {
        public CentreRowUserCtrl()
        {
            InitializeComponent();

            this.WhenActivated(disposables => { 
                RegisterShowDialog(disposables); 
                RegisterCloseOnSelect(disposables); 
            });


        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        // -- Show Dialog --
        protected virtual void RegisterShowDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x=>x.ViewModel)
                .Subscribe(vm=>
                    vm.ShowUpdateDialog.RegisterHandler(UpdateShowDialogAsync) 
                )
            );        
        protected virtual async Task UpdateShowDialogAsync(InteractionContext<CentreUpdateViewModel, dtoo.Centre?> interaction)
        {
            var dialog = new CentreUpdateWindow()
            {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<dtoo.Centre?>(MyVisualRoot());

            interaction.SetOutput(result);
            ( this.Parent as ListBoxItem)!.Focus();
        }

        // -- Select Row
        private Window MyVisualRoot() => (Window)this.VisualRoot;
        private void RegisterCloseOnSelect(Action<IDisposable> disposables)
            =>
            this
            .WhenAnyValue(x=>x.ViewModel)
            .Subscribe(vm=>vm.SeleccionarCommand.Subscribe(MyVisualRoot().Close));

    }
}
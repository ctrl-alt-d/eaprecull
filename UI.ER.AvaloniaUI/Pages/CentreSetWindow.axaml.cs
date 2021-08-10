using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages {
    public class CentreSetWindow : ReactiveWindow<CentreSetViewModel> {
        public CentreSetWindow() {
            InitializeComponent();
            
            DataContext = new CentreSetViewModel();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
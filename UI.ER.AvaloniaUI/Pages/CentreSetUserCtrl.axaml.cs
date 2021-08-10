using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages {
    public class CentreSetUserCtrl : UserControl {
        public CentreSetUserCtrl() {
            InitializeComponent();
            
            DataContext = new CentreSetViewModel();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
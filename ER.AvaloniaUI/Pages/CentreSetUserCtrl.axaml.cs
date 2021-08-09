using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.ViewModels;

namespace ER.AvaloniaUI.Pages {
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
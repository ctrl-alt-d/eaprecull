using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.ViewModels;

namespace ER.AvaloniaUI.Pages {
    public class CentresUserCtrl : UserControl {
        public CentresUserCtrl() {
            InitializeComponent();
            
            DataContext = SuperContext.GetViewModel<CentresViewModel>();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
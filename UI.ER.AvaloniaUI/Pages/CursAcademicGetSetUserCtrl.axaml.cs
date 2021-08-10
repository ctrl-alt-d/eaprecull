using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages {
    public class CursAcademicGetSetUserCtrl : UserControl {
        public CursAcademicGetSetUserCtrl() {
            InitializeComponent();
            
            DataContext = new CursAcademicGetSetViewModel();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
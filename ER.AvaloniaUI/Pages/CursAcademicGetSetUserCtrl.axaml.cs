using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.ViewModels;

namespace ER.AvaloniaUI.Pages {
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract.Services;
using ER.AvaloniaUI.Services;
using ReactiveUI;

namespace ER.AvaloniaUI.Views
{
    public partial class CentresWindow : Window
    {
        public CentresWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
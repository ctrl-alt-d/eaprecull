using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.ViewModels;
using ER.AvaloniaUI.Views;

namespace ER.AvaloniaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
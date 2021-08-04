using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.ViewModels;
using ER.AvaloniaUI.Views;

namespace ER.AvaloniaUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) {
             BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            =>
            AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

        public static MainWindow? MainWindow { get; private set; }
        private static void AppMain(Application app, string[] args) {
            app.Run(SuperContext.GetView<MainWindow>());
        }
    }
}

using System;
using System.Reactive;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace UI.ER.AvaloniaUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            // Prevent ReactiveUI from crashing the app on unhandled exceptions
            // (e.g., when pasting XML/HTML-like text triggers clipboard parsing errors on Windows)
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
            {
                System.Diagnostics.Debug.WriteLine($"[RxApp Unhandled Exception] {ex}");
                // Log but don't crash
            });

            BuildAvaloniaApp()
                // .With(new X11PlatformOptions { UseGpu = false })
                .StartWithClassicDesktopLifetime(args);
        }




        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            =>
            AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            ;

    }
}

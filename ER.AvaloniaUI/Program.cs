using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ER.AvaloniaUI.Services;
using ER.AvaloniaUI.Views;
using ReactiveUI;
using ShowMeTheXaml;
using Avalonia.ReactiveUI;

namespace ER.AvaloniaUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => 
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            =>
            AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseXamlDisplay()
            .LogToTrace()
            .UseReactiveUI()
            ;

    }
}

using System;
using Avalonia;
using ReactiveUI.Avalonia;
using UI.ER.AvaloniaUI.Helpers;

namespace UI.ER.AvaloniaUI;

class Program
{
    public static void Main(string[] args)
    {
        LogHelpers.ConfigureLogging();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogHelpers.LogFatalAndRethrow(ex);
        }
        finally
        {
            LogHelpers.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}

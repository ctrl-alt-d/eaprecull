using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;

namespace UI.ER.AvaloniaUI.Helpers;

public static class LogHelpers
{
    private static readonly string LogFilePath =
        Path.Combine(AppContext.BaseDirectory, "error.log");

    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.File(
                LogFilePath,
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "AppDomain unhandled exception");
            Log.CloseAndFlush();
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
        {
            Log.Error(ex, "RxApp unhandled exception");
        });
    }

    public static void LogFatalAndRethrow(Exception ex)
    {
        Log.Fatal(ex, "Application crashed");
        throw ex;
    }

    public static void CloseAndFlush() => Log.CloseAndFlush();
}

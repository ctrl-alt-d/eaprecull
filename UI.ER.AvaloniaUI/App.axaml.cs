using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.DI;
using DataLayer.DI;
using UI.ER.ViewModels.Services;
using BusinessLayer.Abstract.Generic;
using UI.ER.ViewModels.ViewModels;
using UI.ER.AvaloniaUI.Views;

namespace UI.ER.AvaloniaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices();

            services.AddSingleton<IServiceFactory, SuperContext>();

            var provider = services.BuildServiceProvider();
            SuperContext.Initialize(provider);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
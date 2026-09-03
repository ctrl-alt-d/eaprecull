using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.DI;
using DataLayer.DI;
using UI.ER.AvaloniaUI.DI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.AvaloniaUI.Views;
using UI.ER.ViewModels.Services;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.AvaloniaUI
{
    public class App : Application
    {
        private static IServiceProvider? _services;

        /// <summary>
        /// Provider arrel. Només l'han de fer servir els constructors pont sense
        /// paràmetres de les vistes, que Avalonia instancia des de l'AXAML
        /// (<c>ItemTemplate</c>, previsualitzador) i per tant no passen pel contenidor.
        /// </summary>
        public static IServiceProvider Services
            => _services
               ?? throw new InvalidOperationException(
                   "El contenidor no s'ha construït encara. "
                   + "S'inicialitza a App.OnFrameworkInitializationCompleted().");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .UIConfigureServices();

            services.AddSingleton<IServiceFactory, SuperContext>();

            _services = services.BuildServiceProvider();
            SuperContext.Initialize(_services);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _services
                    .GetRequiredService<IWindowFactory>()
                    .Get<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

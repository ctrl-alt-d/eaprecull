using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using ER.AvaloniaUI.ViewModels;
using ER.AvaloniaUI.DI;
using Avalonia.Controls;
using ER.AvaloniaUI.Views;

namespace ER.AvaloniaUI.Services
{    
    public static class SuperContext
    {
        private static ServiceProvider? _ServiceProvider;
        private static ServiceProvider GetServiceProvider() {
            _ServiceProvider = _ServiceProvider ??
                new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .ViewModelConfigureServices()
                .BuildServiceProvider()
                ;            
            return _ServiceProvider;
        }

        public static T GetBLOperation<T>()
            where T: IBLOperation
        {
            return 
                GetServiceProvider()
                .GetRequiredService<T>();
        }

        public static Avalonia.Controls.Window MainWindow {get;} =  new MainWindow();

    }
}



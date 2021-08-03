using Microsoft.Extensions.DependencyInjection;

using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using ER.AvaloniaUI.ViewModels;

namespace ER.AvaloniaUI.Services
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ViewModelConfigureServices(this IServiceCollection services){
            services.AddTransient<CentresViewModel>();
            services.AddTransient<MainWindowViewModel>();
            return services;
        }
    }

    public static class SuperContext
    {
        private static ServiceProvider? _ServiceProvider;
        public static ServiceProvider GetServiceProvider() {
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

        public static T GetViewModel<T>()
            where T: ViewModelBase
        {
            return 
                GetServiceProvider()
                .GetRequiredService<T>();
        }
    }
}



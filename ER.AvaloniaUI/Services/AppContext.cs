using Microsoft.Extensions.DependencyInjection;

using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;

namespace ER.AvaloniaUI.Services

{
    public static class AppContext
    {
        private static ServiceProvider? _ServiceProvider;
        public static ServiceProvider GetServiceProvider() {
            _ServiceProvider = _ServiceProvider ??
                new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .BuildServiceProvider();            
            return _ServiceProvider;
        }

        public static T GetBLOperation<T>()
            where T: IBLOperation
        {
            return 
                GetServiceProvider()
                .GetRequiredService<T>();
        }


    }
}



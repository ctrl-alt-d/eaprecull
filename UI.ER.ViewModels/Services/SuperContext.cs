using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace UI.ER.ViewModels.Services
{
    public class SuperContext : IServiceFactory
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetBLOperation<T>() where T : IBLOperation
        {
            return GetProvider().GetRequiredService<T>();
        }

        public static T Resolve<T>() where T : IBLOperation
        {
            return GetProvider().GetRequiredService<T>();
        }

        private static IServiceProvider GetProvider()
        {
            return _serviceProvider
                ?? throw new InvalidOperationException("SuperContext no ha estat inicialitzat. Crida SuperContext.Initialize() al Composition Root.");
        }
    }
}



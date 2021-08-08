using Microsoft.Extensions.DependencyInjection;
using ER.AvaloniaUI.ViewModels;
using System;
using System.Linq;
using Avalonia.Controls;

namespace ER.AvaloniaUI.DI
{
    public static class Injection
    {
        public static IServiceCollection ViewModelConfigureServices(this IServiceCollection services)
        {
            // System
            //     .Reflection
            //     .Assembly
            //     .GetExecutingAssembly()
            //     .GetTypes()
            //     .Where(p => typeof(ViewModelBase).IsAssignableFrom(p))
            //     .ToList()
            //     .ForEach(t => services.AddTransient(t));

            // System
            //     .Reflection
            //     .Assembly
            //     .GetExecutingAssembly()
            //     .GetTypes()
            //     .Where(p => typeof(Window).IsAssignableFrom(p))
            //     .ToList()
            //     .ForEach(t => services.AddTransient(t));

            // System
            //     .Reflection
            //     .Assembly
            //     .GetExecutingAssembly()
            //     .GetTypes()
            //     .Where(p => typeof(UserControl).IsAssignableFrom(p))
            //     .ToList()
            //     .ForEach(t => services.AddTransient(t));


            return services;
        }
    }

}
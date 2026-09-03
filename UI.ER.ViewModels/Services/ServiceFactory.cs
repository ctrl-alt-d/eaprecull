using System;
using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace UI.ER.ViewModels.Services
{
    /// <summary>
    /// Única porta d'entrada dels ViewModels al BusinessLayer. Substitueix el Service
    /// Locator estàtic <c>SuperContext</c> que hi havia abans de R1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// És una <em>fàbrica</em>, no un contenidor d'operacions ja construïdes: les
    /// operacions de BL són <c>AddTransient</c> i <c>IBLOperation : IDisposable</c>, i els
    /// ViewModels les consumeixen amb <c>using var bl = …</c> a cada crida. Una instància
    /// injectada quedaria disposada després del primer ús.
    /// </para>
    /// <para>
    /// Es registra com a <c>AddScoped</c> (<see cref="UI.ER.AvaloniaUI.DI.Injection"/>), i
    /// això és el que fa que l'scope per diàleg de la <c>IWindowFactory</c> serveixi de
    /// debò: les operacions transitòries <c>IDisposable</c> queden apuntades a l'scope del
    /// diàleg i s'alliberen en tancar-lo. Resoltes des del provider arrel —el que feia
    /// <c>SuperContext</c>— hi quedaven vives fins a tancar l'aplicació.
    /// </para>
    /// <para>
    /// El tipus genèric està acotat a <see cref="IBLOperation"/>: no és un
    /// <see cref="IServiceProvider"/> disfressat, des d'aquí no s'arriba a cap altre servei.
    /// </para>
    /// </remarks>
    public sealed class ServiceFactory(IServiceProvider provider) : IServiceFactory
    {
        public T GetBLOperation<T>() where T : IBLOperation
            => provider.GetRequiredService<T>();
    }
}

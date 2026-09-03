using System;
using System.Reactive.Linq;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.Services
{
    /// <summary>
    /// Adapta el bus de canvis al costat Rx. El contracte és un <c>event</c> perquè el
    /// BusinessLayer no referencia System.Reactive; qui l'escolta sí que en té.
    /// </summary>
    public static class NotificadorExtensions
    {
        public static IObservable<CanviDeDomini> ComObservable(this INotificadorDeCanvis notificador)
            => Observable.FromEvent<CanviDeDomini>(
                h => notificador.Canvi += h,
                h => notificador.Canvi -= h);
    }
}

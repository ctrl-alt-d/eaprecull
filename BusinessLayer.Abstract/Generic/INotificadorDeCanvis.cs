using System;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// Bus de canvis de domini: el punt on les quatre classes base d'escriptura del
    /// BusinessLayer diuen què han tocat, i on les llistes obertes ho escolten.
    /// </summary>
    /// <remarks>
    /// Amb <c>event</c> i no amb <c>IObservable</c>: ni <c>BusinessLayer</c> ni
    /// <c>BusinessLayer.Abstract</c> referencien System.Reactive, i el bus no és motiu
    /// per fer-los-hi dependre. El costat UI l'adapta amb <c>Observable.FromEvent</c>.
    /// </remarks>
    public interface INotificadorDeCanvis
    {
        event Action<CanviDeDomini>? Canvi;

        void Publica(CanviDeDomini canvi);
    }
}

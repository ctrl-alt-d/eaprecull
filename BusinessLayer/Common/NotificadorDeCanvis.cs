using System;
using System.Linq;
using BusinessLayer.Abstract.Generic;
using Serilog;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Implementació del bus de canvis. Singleton: hi ha un sol bus per a tota
    /// l'aplicació, i les operacions —que són Transient— el reben per propietat des del
    /// registre (<c>BusinessLayer/DI/Injection.cs</c>).
    /// </summary>
    public sealed class NotificadorDeCanvis : INotificadorDeCanvis
    {
        public event Action<CanviDeDomini>? Canvi;

        public void Publica(CanviDeDomini canvi)
        {
            // Captura del delegat abans d'invocar-lo: un subscriptor es pot donar de baixa
            // enmig de la notificació (és el que passa en tancar-se una finestra).
            var subscriptors = Canvi;

            if (subscriptors is null)
                return;

            // Un a un i dins d'un try/catch: qui peti no ha de tombar ni els altres
            // subscriptors ni l'operació de BusinessLayer que ha publicat el canvi.
            foreach (var subscriptor in subscriptors.GetInvocationList().Cast<Action<CanviDeDomini>>())
            {
                try
                {
                    subscriptor(canvi);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error en notificar un canvi de domini");
                }
            }
        }
    }
}

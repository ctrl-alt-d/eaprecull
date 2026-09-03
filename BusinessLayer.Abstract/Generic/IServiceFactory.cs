namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// El port únic dels ViewModels cap al BusinessLayer, amb dues cares: demanar-li
    /// operacions i escoltar-ne els canvis. Segueix sense donar accés a cap altre servei.
    /// </summary>
    public interface IServiceFactory
    {
        T GetBLOperation<T>() where T : IBLOperation;

        /// <summary>El bus on el BusinessLayer publica què ha escrit.</summary>
        INotificadorDeCanvis Canvis { get; }
    }
}

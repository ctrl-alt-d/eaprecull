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

        /// <summary>
        /// Les dades de qui fa servir el programa. Hi és per la mateixa raó que
        /// <see cref="Canvis"/>: <see cref="GetBLOperation{T}"/> està acotat a
        /// <see cref="IBLOperation"/> i no el pot tornar, i injectar-lo pel constructor
        /// del ViewModel el faria semblar un ViewModel amb arguments de runtime.
        /// </summary>
        IDadesDeLusuari DadesUsuari { get; }
    }
}

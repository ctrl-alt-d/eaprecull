using CommonInterfaces;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// Qui fa servir el programa. Implementa <see cref="IEtiquetaDescripcio"/> perquè
    /// encaixi a <c>OperationResult&lt;T&gt;</c>, que és com la resta del BusinessLayer
    /// torna errors.
    /// </summary>
    public sealed record DadesUsuari(string Nom, string Cognoms, string AdrecaXtec)
        : IEtiquetaDescripcio
    {
        /// <summary>El que es té quan el fitxer no hi és, no es pot llegir o és escombraria.</summary>
        public static DadesUsuari Buides { get; } = new(string.Empty, string.Empty, string.Empty);

        public string Etiqueta => $"{Nom} {Cognoms}".Trim();

        public string Descripcio => AdrecaXtec;
    }
}

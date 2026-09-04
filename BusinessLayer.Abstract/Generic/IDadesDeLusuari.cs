using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// Les dades de qui fa servir el programa, llegides d'un <c>.ini</c> al costat de la
    /// base de dades. Singleton: un sol fitxer per a tota l'aplicació.
    /// </summary>
    /// <remarks>
    /// No és una <see cref="IBLOperation"/>: no és d'un sol ús, no toca la base de dades i
    /// no publica al bus de canvis. Segueix el patró d'<see cref="INotificadorDeCanvis"/>,
    /// i com ell viu a <c>Generic</c> per quedar fora de l'escaneig d'operacions, que
    /// filtra pel namespace <c>BusinessLayer.Abstract.Services</c>.
    /// <para>
    /// El fitxer no es llegeix fins al primer accés: cap <c>*ConfigureServices</c> ni cap
    /// resolució del contenidor no pot tocar el sistema de fitxers.
    /// </para>
    /// </remarks>
    public interface IDadesDeLusuari
    {
        /// <summary>Mai null: si el fitxer no hi és, són <see cref="DadesUsuari.Buides"/>.</summary>
        DadesUsuari Actuals { get; }

        /// <summary>Fals si el fitxer no existeix, no es pot llegir o li falta algun camp.</summary>
        bool EstaInformat { get; }

        /// <summary>El camí del fitxer, per poder-lo dir a l'usuari.</summary>
        string Ubicacio { get; }

        /// <summary>
        /// Valida i desa. Si hi ha <c>BrokenRules</c> no toca el disc i
        /// <see cref="Actuals"/> no canvia.
        /// </summary>
        Task<OperationResult<DadesUsuari>> Desa(DadesUsuari dades);
    }
}

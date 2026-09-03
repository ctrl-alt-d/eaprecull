using System.Collections.Generic;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// Les entitats del domini que una llista pot tenir pintades. El nom de cada valor és
    /// el del DTO i el del model d'EF: la correspondència tipus → entitat es fa pel nom
    /// (veure <see cref="Referencies.EntitatDe"/>), no per cap taula escrita a mà.
    /// </summary>
    public enum Entitat
    {
        Actuacio,
        Alumne,
        Centre,
        CursAcademic,
        Etapa,
        TipusActuacio,
    }

    /// <summary>Què li ha passat a l'entitat.</summary>
    public enum MenaDeCanvi
    {
        Alta,
        Modificacio,
        Baixa,

        /// <summary>Operació massiva: no se'n saben les entitats afectades.</summary>
        Massiu,
    }

    /// <summary>Una entitat concreta: el parell (tipus, id).</summary>
    public readonly record struct Referencia(Entitat Entitat, int Id);

    /// <summary>
    /// El que el BusinessLayer publica després de cada escriptura que ha anat bé.
    /// <paramref name="Afectats"/> són les entitats que el DTO escrit referencia, calculades
    /// amb <see cref="Referencies"/>; qui escolta compara aquest conjunt amb el de les seves
    /// files i es refresca si s'intersequen.
    /// </summary>
    public sealed record CanviDeDomini(MenaDeCanvi Mena, IReadOnlySet<Referencia> Afectats)
    {
        /// <summary>Una operació massiva: no se'n saben les entitats, s'ha de refrescar tot.</summary>
        public static CanviDeDomini Tot { get; } = new(MenaDeCanvi.Massiu, new HashSet<Referencia>());

        public bool AfectaTot => Mena == MenaDeCanvi.Massiu;
    }
}

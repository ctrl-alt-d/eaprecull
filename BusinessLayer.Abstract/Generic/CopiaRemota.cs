using System;
using System.Collections.Generic;
using System.Linq;
using CommonInterfaces;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>Una còpia que ja és al destí.</summary>
    public sealed record CopiaRemota(string Id, string Nom, DateTime Creada, long Bytes)
        : IEtiquetaDescripcio
    {
        public string Etiqueta => Nom;

        public string Descripcio => $"{Creada:dd/MM/yyyy HH:mm} · {Bytes / 1024d / 1024d:N1} MB";
    }

    /// <summary>
    /// Embolcall d'una llista de còpies: <see cref="OperationResult{T}"/> exigeix
    /// <c>T : IEtiquetaDescripcio</c> i una llista pelada no ho compleix. Mateix cas que
    /// <c>ImportAllResult</c>.
    /// </summary>
    public sealed record Copies(IReadOnlyList<CopiaRemota> Items) : IEtiquetaDescripcio
    {
        /// <summary>El que es té quan encara no s'ha mirat el destí.</summary>
        public static Copies Cap { get; } = new(Array.Empty<CopiaRemota>());

        public string Etiqueta => $"{Items.Count} còpies";

        public string Descripcio => string.Join(" · ", Items.Select(c => c.Nom));
    }

    /// <summary>
    /// On van les còpies, tal com s'ha de poder ensenyar a l'usuari. Per a la carpeta,
    /// «Carpeta» + el camí; per al Drive, el correu del compte + la carpeta remota.
    /// </summary>
    public sealed record DestiDeCopies(string Nom, string Detall) : IEtiquetaDescripcio
    {
        public static DestiDeCopies Cap { get; } = new(string.Empty, "Cap destí configurat");

        public bool Configurat => !string.IsNullOrEmpty(Nom);

        public string Etiqueta => Nom;

        public string Descripcio => Detall;
    }

    /// <summary>
    /// Si toca proposar una còpia en arrencar, i per què. Es proposa quan fa més de dues
    /// setmanes de l'última <strong>i</strong> hi ha actuacions noves des d'aleshores:
    /// sense feina nova a perdre, una còpia no aporta res i un recordatori que surt sense
    /// motiu només ensenya a ignorar-lo.
    /// </summary>
    /// <param name="ActuacionsNoves">Les que s'han fet des de l'última còpia.</param>
    /// <param name="Dies">Els que fa que no se'n fa cap, o 0 si no se n'ha fet mai.</param>
    /// <param name="MaiSHaFetCap">No hi ha cap còpia anterior amb què comparar.</param>
    public sealed record PropostaDeCopia(bool Cal, int ActuacionsNoves, int Dies, bool MaiSHaFetCap)
        : IEtiquetaDescripcio
    {
        /// <summary>No cal proposar res: o és recent, o no hi ha feina nova.</summary>
        public static PropostaDeCopia No { get; } = new(false, 0, 0, false);

        public string Etiqueta => Cal
            ? "Et proposem fer una còpia de seguretat"
            : "Les còpies estan al dia";

        public string Descripcio
        {
            get
            {
                if (!Cal)
                    return string.Empty;

                var noves = ActuacionsNoves == 1
                    ? "1 actuació nova"
                    : $"{ActuacionsNoves:N0} actuacions noves";

                if (MaiSHaFetCap)
                    return $"Encara no has fet cap còpia de seguretat i ja tens {noves}. "
                         + "Et proposem fer-ne una ara.";

                var dies = Dies == 1 ? "1 dia" : $"{Dies:N0} dies";

                return $"Han passat {dies} des de la darrera còpia i has fet {noves}. "
                     + "Et proposem fer una còpia de seguretat.";
            }
        }
    }

    /// <summary>
    /// Un destí que hi ha en aquesta compilació, per pintar el selector de la finestra
    /// sense que la UI hagi de conèixer cap <see cref="IMagatzemDeCopies"/>.
    /// </summary>
    /// <param name="Clau">El que es desa a l'<c>Usuari.ini</c>: «carpeta», «drive»…</param>
    /// <param name="Titol">Com es diu a la pantalla: «Carpeta», «Google Drive».</param>
    /// <param name="DemanaCarpeta">
    /// Si el paràmetre que necessita és una carpeta local, i per tant la finestra hi ha
    /// d'oferir el selector de carpetes. Fals per als destins que s'autoritzen sols.
    /// </param>
    public sealed record DestiDisponible(string Clau, string Titol, bool DemanaCarpeta)
        : IEtiquetaDescripcio
    {
        public string Etiqueta => Titol;

        public string Descripcio => Clau;
    }
}

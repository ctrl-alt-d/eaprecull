using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// On van a parar les còpies. La interfície no diu enlloc si és una carpeta o un
    /// núvol, i és el que permet provar l'operació sense xarxa i afegir destins nous
    /// sense tocar-la.
    /// </summary>
    /// <remarks>
    /// Singleton: la configuració —i, al Drive, la sessió autoritzada— és una de sola per
    /// a tot el programa. El constructor <strong>no</strong> pot tocar ni el disc ni la
    /// xarxa, igual que <see cref="IDadesDeLusuari"/>: <c>InjeccioTest</c> construeix el
    /// contenidor sencer i resol totes les operacions, i una d'elles el demana pel
    /// constructor.
    /// <para>
    /// Viu a <c>Generic</c> i no a <c>Services</c> perquè no és una operació de negoci:
    /// l'escaneig del registre filtra pel namespace i no l'ha de veure.
    /// </para>
    /// </remarks>
    public interface IMagatzemDeCopies
    {
        /// <summary>«carpeta», «drive»… El que es desa a l'<c>Usuari.ini</c> per triar destí.</summary>
        string Clau { get; }

        /// <summary>Com es diu, per pintar-ho al selector: «Carpeta», «Google Drive».</summary>
        string Titol { get; }

        /// <summary>
        /// Si el paràmetre que li cal és una carpeta local. Ho ha de saber la finestra per
        /// decidir si ofereix el selector de carpetes o un botó de connexió.
        /// </summary>
        bool DemanaCarpeta { get; }

        /// <summary>On van les còpies ara mateix, sense obrir res ni connectar enlloc.</summary>
        DestiDeCopies Desti { get; }

        /// <summary>
        /// Desa la tria de l'usuari —la carpeta triada, el compte autoritzat— i deixa el
        /// magatzem llest. És l'únic lloc on entra el <paramref name="parametre"/>: quin
        /// significat té és cosa de cada adaptador, i per això l'operació de negoci se'l
        /// passa tal com l'hi ha donat la UI sense mirar-se'l.
        /// </summary>
        Task<OperationResult<DestiDeCopies>> Configura(string? parametre, CancellationToken ct);

        /// <summary>
        /// Deixa el magatzem llest amb la configuració que ja tenia: la carpeta comprova
        /// que existeix i s'hi pot escriure; el Drive obre el navegador si no té
        /// credencials vives. Idempotent.
        /// </summary>
        Task<OperationResult<DestiDeCopies>> Prepara(CancellationToken ct);

        /// <summary>Hi desa un fitxer local. Torna la còpia tal com ha quedat al destí.</summary>
        Task<OperationResult<CopiaRemota>> Desa(
            string camiLocal, string nomDesti, IProgress<long>? bytesEscrits, CancellationToken ct);

        /// <summary>Les còpies que hi ha, de la més nova a la més vella.</summary>
        Task<OperationResult<Copies>> Llista(CancellationToken ct);

        /// <summary>Deixa només les <paramref name="quantes"/> més noves. Torna les retirades.</summary>
        Task<OperationResult<Copies>> RetiraSobrants(int quantes, CancellationToken ct);

        /// <summary>Oblida la configuració (carpeta triada, credencials desades). Idempotent.</summary>
        Task<OperationResult<DestiDeCopies>> Oblida();
    }
}

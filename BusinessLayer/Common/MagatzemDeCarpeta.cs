using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using Serilog;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Desa les còpies a una carpeta que tria l'usuari. És el camí principal, i el mateix
    /// codi cobreix tres casos: un llapis USB, una unitat de xarxa i —aquí és on puja al
    /// núvol— la carpeta local d'un client de sincronització ja instal·lat i ja autoritzat
    /// (Google Drive per a ordinadors, OneDrive, Nextcloud…).
    /// </summary>
    /// <remarks>
    /// Aquesta última és la gràcia: EAP Recull no parla amb cap núvol ni demana permís a
    /// cap administrador de domini; hi parla el client de sincronització, que ja hi
    /// parlava abans. Zero dependències, zero credencials.
    /// <para>
    /// El constructor no toca el disc —ni tan sols per saber quina carpeta hi ha
    /// configurada—: <c>InjeccioTest</c> construeix el contenidor sencer i resol totes les
    /// operacions, i <c>CopiaDeSeguretat</c> demana els magatzems pel constructor.
    /// </para>
    /// </remarks>
    public sealed class MagatzemDeCarpeta : IMagatzemDeCopies
    {
        /// <summary>La clau que es desa a l'<c>Usuari.ini</c> per triar aquest destí.</summary>
        public const string ClauDelDesti = "carpeta";

        /// <summary>La clau de l'<c>Usuari.ini</c> on viu la carpeta triada.</summary>
        public const string ClauDeLaCarpeta = "Carpeta";

        private readonly ConfiguracioDeCopies _configuracio;

        public MagatzemDeCarpeta(IDadesDeLusuari usuari)
            => _configuracio = new ConfiguracioDeCopies(usuari);

        public string Clau => ClauDelDesti;

        public string Titol => "Carpeta";

        public bool DemanaCarpeta => true;

        /// <summary>La carpeta configurada, o null si encara no se n'ha triat cap.</summary>
        private string? Carpeta => _configuracio.Valor(ClauDeLaCarpeta);

        public DestiDeCopies Desti
            => Carpeta is { } carpeta ? new DestiDeCopies(Titol, carpeta) : DestiDeCopies.Cap;

        public Task<OperationResult<DestiDeCopies>> Configura(string? parametre, CancellationToken ct)
        {
            var carpeta = (parametre ?? string.Empty).Trim();

            if (carpeta.Length == 0)
                return Trencada<DestiDeCopies>("Tria on vols desar les còpies abans de fer-ne cap.");

            try
            {
                // Es normalitza abans de desar-la: així el que es compara amb la carpeta
                // de dades i el que s'ensenya a l'usuari són sempre la mateixa cadena.
                carpeta = Path.GetFullPath(carpeta);
            }
            catch (Exception e)
            {
                Log.Error(e, "Carpeta de còpies no vàlida: {Carpeta}", carpeta);
                return Trencada<DestiDeCopies>($"«{carpeta}» no és una carpeta vàlida.");
            }

            var comprovacio = ComprovaLaCarpeta(carpeta);
            if (comprovacio is not null)
                return Trencada<DestiDeCopies>(comprovacio);

            try
            {
                _configuracio.Assigna(ClauDeLaCarpeta, carpeta);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error desant la carpeta de còpies a l'Usuari.ini");
                return Trencada<DestiDeCopies>($"No s'ha pogut desar la carpeta triada: {e.Message}");
            }

            return Task.FromResult(new OperationResult<DestiDeCopies>(new DestiDeCopies(Titol, carpeta)));
        }

        public Task<OperationResult<DestiDeCopies>> Prepara(CancellationToken ct)
        {
            if (Carpeta is not { } carpeta)
                return Trencada<DestiDeCopies>("Tria on vols desar les còpies abans de fer-ne cap.");

            var comprovacio = ComprovaLaCarpeta(carpeta);

            return comprovacio is null
                ? Task.FromResult(new OperationResult<DestiDeCopies>(new DestiDeCopies(Titol, carpeta)))
                : Trencada<DestiDeCopies>(comprovacio);
        }

        public async Task<OperationResult<CopiaRemota>> Desa(
            string camiLocal, string nomDesti, IProgress<long>? bytesEscrits, CancellationToken ct)
        {
            if (Carpeta is not { } carpeta)
                return Trenca<CopiaRemota>("Tria on vols desar les còpies abans de fer-ne cap.");

            var desti = Path.Combine(carpeta, nomDesti);

            // Escriptura atòmica: primer a un nom que el patró de Llista() no veu, i
            // després un Move. Un client de sincronització no ha de començar a pujar mai
            // un fitxer a mig escriure.
            var temporal = desti + NomDeCopia.ExtensioTemporal;

            try
            {
                await CopiaAmbProgres(camiLocal, temporal, bytesEscrits, ct);

                File.Move(temporal, desti, overwrite: true);

                var informacio = new FileInfo(desti);

                return new OperationResult<CopiaRemota>(
                    new CopiaRemota(desti, nomDesti, informacio.LastWriteTime, informacio.Length));
            }
            catch (OperationCanceledException)
            {
                // Cancel·lar no és un error de disc: qui n'ha de donar el missatge és
                // l'operació, que és qui sap què s'estava fent.
                Esborra(temporal);
                throw;
            }
            catch (Exception e)
            {
                Esborra(temporal);

                Log.Error(e, "Error desant la còpia a {Desti}", desti);
                return Trenca<CopiaRemota>(MissatgeDe(e, carpeta));
            }
        }

        public Task<OperationResult<Copies>> Llista(CancellationToken ct)
        {
            if (Carpeta is not { } carpeta)
                return Task.FromResult(new OperationResult<Copies>(Copies.Cap));

            try
            {
                if (!Directory.Exists(carpeta))
                    return Trencada<Copies>(
                        $"No es troba la carpeta {carpeta}. Comprova que el llapis o la unitat de xarxa hi siguin.");

                // El patró del nom és el que delimita què és nostre: a la carpeta que ha
                // triat l'usuari hi pot haver el que sigui, i no s'hi toca res més.
                var copies = Directory
                    .EnumerateFiles(carpeta, NomDeCopia.Patro, SearchOption.TopDirectoryOnly)
                    .Where(f => NomDeCopia.EsNostre(Path.GetFileName(f)))
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new CopiaRemota(f.FullName, f.Name, f.LastWriteTime, f.Length))
                    .ToList();

                return Task.FromResult(new OperationResult<Copies>(new Copies(copies)));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error llistant les còpies de {Carpeta}", carpeta);
                return Trencada<Copies>(MissatgeDe(e, carpeta));
            }
        }

        public async Task<OperationResult<Copies>> RetiraSobrants(int quantes, CancellationToken ct)
        {
            var llistat = await Llista(ct);

            if (llistat.BrokenRules.Count > 0)
                return new OperationResult<Copies>(llistat.BrokenRules);

            var sobrants = llistat.Data!.Items.Skip(quantes).ToList();
            var retirades = new List<CopiaRemota>();
            var problemes = new List<BrokenRule>();

            foreach (var copia in sobrants)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    // Definitiu: a diferència del Drive, aquí no hi ha paperera fiable a
                    // totes les plataformes. Per això la finestra diu quantes se'n guarden.
                    File.Delete(copia.Id);
                    retirades.Add(copia);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error retirant la còpia antiga {Copia}", copia.Id);
                    problemes.Add(new BrokenRule($"No s'ha pogut esborrar {copia.Nom}: {e.Message}"));
                }
            }

            return problemes.Count > 0
                ? new OperationResult<Copies>(problemes)
                : new OperationResult<Copies>(new Copies(retirades));
        }

        /// <summary>Oblida la carpeta triada. <strong>No esborra cap fitxer.</strong></summary>
        public Task<OperationResult<DestiDeCopies>> Oblida()
        {
            try
            {
                _configuracio.Assigna(ClauDeLaCarpeta, null);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error oblidant la carpeta de còpies");
                return Trencada<DestiDeCopies>($"No s'ha pogut oblidar la carpeta: {e.Message}");
            }

            return Task.FromResult(new OperationResult<DestiDeCopies>(DestiDeCopies.Cap));
        }

        /// <summary>
        /// La carpeta existeix i s'hi pot escriure? La prova d'escriptura és de debò —crear
        /// i esborrar un fitxer—: un permís de només lectura no es veu de cap altra manera,
        /// i val més descobrir-ho abans de bolcar la base de dades que després.
        /// </summary>
        private static string? ComprovaLaCarpeta(string carpeta)
        {
            if (!Directory.Exists(carpeta))
                return $"No es troba la carpeta {carpeta}. Comprova que el llapis o la unitat de xarxa hi siguin.";

            var prova = Path.Combine(carpeta, $".eaprecull-{Guid.NewGuid():N}.tmp");

            try
            {
                using (File.Create(prova)) { }
                File.Delete(prova);
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, "No es pot escriure a la carpeta de còpies {Carpeta}", carpeta);
                Esborra(prova);
                return MissatgeDe(e, carpeta);
            }
        }

        /// <summary>
        /// Còpia amb <see cref="IProgress{T}"/>: ni un llapis USB ni una unitat de xarxa
        /// són instantanis, i la finestra ha de poder dir alguna cosa mentre dura.
        /// </summary>
        private static async Task CopiaAmbProgres(
            string origen, string desti, IProgress<long>? bytesEscrits, CancellationToken ct)
        {
            var buffer = new byte[81920];
            long total = 0;

            using var lectura = new FileStream(
                origen, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, useAsync: true);
            using var escriptura = new FileStream(
                desti, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true);

            int llegits;
            while ((llegits = await lectura.ReadAsync(buffer, ct)) > 0)
            {
                await escriptura.WriteAsync(buffer.AsMemory(0, llegits), ct);

                total += llegits;
                bytesEscrits?.Report(total);
            }

            await escriptura.FlushAsync(ct);
        }

        private static void Esborra(string cami)
        {
            try
            {
                if (File.Exists(cami))
                    File.Delete(cami);
            }
            catch (Exception e)
            {
                // Un temporal que es queda és lleig, però no és motiu per tombar res: el
                // que importa és l'error original que ens ha portat fins aquí.
                Log.Warning(e, "No s'ha pogut esborrar el fitxer temporal {Cami}", cami);
            }
        }

        /// <summary>
        /// Tradueix l'excepció al que l'usuari ha de llegir. El camí hi va sempre: sense
        /// ell, «no es pot escriure» no li diu si el problema és el llapis o el servidor.
        /// </summary>
        private static string MissatgeDe(Exception e, string carpeta) => e switch
        {
            DirectoryNotFoundException =>
                $"No es troba la carpeta {carpeta}. Comprova que el llapis o la unitat de xarxa hi siguin.",
            UnauthorizedAccessException => $"No es pot escriure a {carpeta}.",
            IOException io when EsDiscPle(io) => "No hi ha prou espai per desar la còpia.",
            IOException io => $"No es pot escriure a {carpeta}: {io.Message}",
            _ => $"No s'ha pogut desar la còpia a {carpeta}: {e.Message}",
        };

        /// <summary>
        /// Disc ple: <c>ERROR_DISK_FULL</c>/<c>ERROR_HANDLE_DISK_FULL</c> a Windows i
        /// <c>ENOSPC</c> a Unix. .NET no té cap excepció pròpia per a aquest cas.
        /// </summary>
        private static bool EsDiscPle(IOException e)
            => (e.HResult & 0xFFFF) is 0x27 or 0x70 or 28;

        private static Task<OperationResult<T>> Trencada<T>(string missatge)
            where T : CommonInterfaces.IEtiquetaDescripcio
            => Task.FromResult(Trenca<T>(missatge));

        private static OperationResult<T> Trenca<T>(string missatge)
            where T : CommonInterfaces.IEtiquetaDescripcio
            => new(new List<BrokenRule> { new(missatge) });
    }
}

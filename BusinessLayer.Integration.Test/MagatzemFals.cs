using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Common;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// El doble del port. Desa a una carpeta temporal i deixa fer-lo fallar a voluntat:
    /// és el que permet provar l'operació sencera —bolcat, verificació, xifratge,
    /// retenció, ordre de les passes— <strong>sense xarxa i sense navegador</strong>.
    /// </summary>
    internal sealed class MagatzemFals : IMagatzemDeCopies, IDisposable
    {
        public MagatzemFals()
        {
            Carpeta = Path.Combine(Path.GetTempPath(), "EapRecullTest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Carpeta);
        }

        public string Carpeta { get; }

        public string Clau => "fals";
        public string Titol => "Magatzem fals";
        public bool DemanaCarpeta => false;

        public DestiDeCopies Desti => new(Titol, Carpeta);

        // --- Les palanques per fer-lo fallar --------------------------------------

        public string? FallaAlPreparar { get; set; }
        public string? FallaAlDesar { get; set; }
        public string? FallaAlRetirar { get; set; }

        // --- El que ha passat, per comprovar-ho -----------------------------------

        public int VegadesQueSHaPreparat { get; private set; }
        public int VegadesQueSHaDesat { get; private set; }
        public int VegadesQueSHaRetirat { get; private set; }
        public long BytesReportats { get; private set; }

        public IReadOnlyList<string> Fitxers
            => Directory.EnumerateFiles(Carpeta).Select(Path.GetFileName).OfType<string>()
                .OrderBy(f => f, StringComparer.Ordinal).ToList();

        // --- El port ---------------------------------------------------------------

        public Task<OperationResult<DestiDeCopies>> Configura(string? parametre, CancellationToken ct)
            => Task.FromResult(new OperationResult<DestiDeCopies>(Desti));

        public Task<OperationResult<DestiDeCopies>> Prepara(CancellationToken ct)
        {
            VegadesQueSHaPreparat++;

            return Task.FromResult(FallaAlPreparar is { } motiu
                ? new OperationResult<DestiDeCopies>(new List<BrokenRule> { new(motiu) })
                : new OperationResult<DestiDeCopies>(Desti));
        }

        public Task<OperationResult<CopiaRemota>> Desa(
            string camiLocal, string nomDesti, IProgress<long>? bytesEscrits, CancellationToken ct)
        {
            VegadesQueSHaDesat++;

            if (FallaAlDesar is { } motiu)
                return Task.FromResult(
                    new OperationResult<CopiaRemota>(new List<BrokenRule> { new(motiu) }));

            var desti = Path.Combine(Carpeta, nomDesti);
            File.Copy(camiLocal, desti, overwrite: true);

            var informacio = new FileInfo(desti);
            BytesReportats = informacio.Length;
            bytesEscrits?.Report(informacio.Length);

            return Task.FromResult(new OperationResult<CopiaRemota>(
                new CopiaRemota(desti, nomDesti, informacio.LastWriteTime, informacio.Length)));
        }

        public Task<OperationResult<Copies>> Llista(CancellationToken ct)
        {
            var copies = Directory
                .EnumerateFiles(Carpeta, NomDeCopia.Patro)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => new CopiaRemota(f.FullName, f.Name, f.LastWriteTime, f.Length))
                .ToList();

            return Task.FromResult(new OperationResult<Copies>(new Copies(copies)));
        }

        public async Task<OperationResult<Copies>> RetiraSobrants(int quantes, CancellationToken ct)
        {
            VegadesQueSHaRetirat++;

            if (FallaAlRetirar is { } motiu)
                return new OperationResult<Copies>(new List<BrokenRule> { new(motiu) });

            var llistat = await Llista(ct);
            var sobrants = llistat.Data!.Items.Skip(quantes).ToList();

            foreach (var copia in sobrants)
                File.Delete(copia.Id);

            return new OperationResult<Copies>(new Copies(sobrants));
        }

        public Task<OperationResult<DestiDeCopies>> Oblida()
            => Task.FromResult(new OperationResult<DestiDeCopies>(DestiDeCopies.Cap));

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Carpeta))
                    Directory.Delete(Carpeta, recursive: true);
            }
            catch (IOException)
            {
                // Un temporal que es queda no ha de fer vermell cap test.
            }
        }
    }
}

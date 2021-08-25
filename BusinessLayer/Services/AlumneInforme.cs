using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DataModels.Models;
using Microsoft.EntityFrameworkCore;
using SharpDocx;

namespace BusinessLayer.Services
{

    public class AlumneInforme : BLOperation, IAlumneInforme
    {

        public AlumneInforme(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        public async Task<StringOperationResult> Run(int alumneId, string? path = null)
        {
            try
            {
                var dades = await GetDadesAlumne(alumneId);

                if (dades == null) throw new BrokenRuleException("Alumne no trobat");

                var cognomsnom = $"{dades.Cognoms}_{dades.Nom}".Replace(" ", "_");

                path ??= CalculaPath(cognomsnom);
                var templatepath = GetTemplatesPath("AlumneInforme.cs.docx");

                var document = DocumentFactory.Create(templatepath, dades,forceCompile: true);
                document.Generate(path);



            }
            catch (BrokenRuleException e)
            {

                return new StringOperationResult(e.BrokenRules);
            }
            catch (Exception e)
            {
                var msg = e.Message;
                return new StringOperationResult(new List<BrokenRule>() { new BrokenRule(msg)});
            }

            return new StringOperationResult(path);
        }

        private Task<Alumne> GetDadesAlumne(int alumneId)
            =>
            GetContext()
            .Alumnes
            .Include(a => a.Actuacions.OrderByDescending(x=>x.MomentDeLactuacio))
            .Include(a => a.Actuacions).ThenInclude(a => a.CursActuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.CentreAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.EtapaAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.TipusActuacio)
            .Include(a => a.CentreActual)
            .Include(a => a.EtapaActual)
            .Where(a => a.Id == alumneId)
            .FirstOrDefaultAsync();

        private static string CalculaPath(string prefixfilename)
        {

            var filename = $"{prefixfilename}-{DateTime.Now.ToString("yyyyMMdd")}.docx";
            var directori = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");

#if (DEBUG)
            // En mode debug a la carpeta de documents.
            directori = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EapRecullReports");
#endif

            if (!Directory.Exists(directori))
            {
                Directory.CreateDirectory(directori);
            }
            var path = Path.Combine(directori, filename);

            while (File.Exists(path))
            {
                path = path.Replace(".docx", "_nou.docx");
            }

            return path;
        }

        private static string GetTemplatesPath(string template)
        {

            var directori = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");


            if (!Directory.Exists(directori))
            {
                throw new Exception($"No trobo la carpeta: {directori}");
            }
            var path = Path.Combine(directori, template);

            if (!File.Exists(path))
            {
                throw new Exception($"No trobat el template {path}");
            }

            return path;
        }


    }
}
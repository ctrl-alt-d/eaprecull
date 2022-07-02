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
using dtoo = DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;
using SharpDocx;

namespace BusinessLayer.Services
{

    public class AlumneInforme : BLOperation, IAlumneInforme
    {

        public AlumneInforme(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        public async Task<OperationResult<dtoo.SaveResult>> Run(int alumneId)
        {
            try
            {
                var dades = await GetDadesAlumne(alumneId);

                if (dades == null) throw new BrokenRuleException("Alumne no trobat");

                var cognomsnom = $"{dades.Cognoms}_{dades.Nom}".Replace(" ", "_");

                var (path, filename, folder) = CalculaPath(cognomsnom);
                var templatepath = GetTemplatesPath("AlumneInforme.cs.docx");
                var document = DocumentFactory.Create(templatepath, dades);


                document.Generate(path);

                return new(
                    new dtoo.SaveResult(
                        path,
                        filename,
                        folder
                    )
                );

            }
            catch (BrokenRuleException e)
            {

                return new(e.BrokenRules);
            }
            catch (Exception e)
            {
                var msg = e.Message;
                System.Console.WriteLine(e.StackTrace);
                return new(new List<BrokenRule>() { new BrokenRule(msg) });
            }

        }

        private Task<Alumne?> GetDadesAlumne(int alumneId)
            =>
            GetContext()
            .Alumnes
            .Include(a => a.Actuacions.OrderByDescending(x => x.MomentDeLactuacio))
            .Include(a => a.Actuacions).ThenInclude(a => a.CursActuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.CentreAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.EtapaAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.TipusActuacio)
            .Include(a => a.CentreActual)
            .Include(a => a.EtapaActual)
            .Where(a => a.Id == alumneId)
            .FirstOrDefaultAsync();

        private static (string path, string filename, string folder) CalculaPath(string prefixfilename)
        {

            var filename = $"{prefixfilename}-{DateTime.Now.ToString("yyyyMMdd")}.docx";

            var binPath = AppDomain.CurrentDomain.BaseDirectory;

            var folder = Path.Combine(binPath, "Reports");

#if (DEBUG)
            // En mode debug a la carpeta de documents.
            folder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EapRecullReports");
#endif

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var path = string.Empty;
            while (true)
            {
                path = Path.Combine(folder, filename);
                if (!File.Exists(path)) break;
                filename = filename.Replace(".docx", "_nou.docx");
            }

            return (path, filename, folder);
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
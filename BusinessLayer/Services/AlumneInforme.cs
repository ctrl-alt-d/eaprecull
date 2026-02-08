using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DataModels.Models;
using Dtoo = DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;
using SharpDocx;

namespace BusinessLayer.Services
{
    public class AlumneInforme : BLReport<Dtoo.SaveResult>, IAlumneInforme
    {
        public AlumneInforme(IDbContextFactory<AppDbContext> appDbContextFactory)
            : base(appDbContextFactory)
        {
        }

        public Task<OperationResult<Dtoo.SaveResult>> Run(int alumneId)
            => ExecuteReport(() => GenerateReport(alumneId));

        private async Task<Dtoo.SaveResult> GenerateReport(int alumneId)
        {
            var dades = await GetDadesAlumne(alumneId);

            if (dades == null)
                throw new BrokenRuleException("Alumne no trobat");

            var cognomsnom = $"{dades.Cognoms}_{dades.Nom}".Replace(" ", "_");

            var (path, filename, folder) = CalculatePath(cognomsnom, "docx");
            var templatePath = GetTemplatesPath("AlumneInforme.cs.docx");
            var document = DocumentFactory.Create(templatePath, dades);

            document.Generate(path);

            return new Dtoo.SaveResult(path, filename, folder);
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
    }
}
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Threading.Tasks;
using DataModels.Models;
using System.Linq;

namespace BusinessLayer.Services
{
    public class CursAcademicCreate : 
        BLCreate<models.CursAcademic, parms.CursAcademicCreateParms, dtoo.CursAcademic>,
        ICursAcademicCreate
    {
        public CursAcademicCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(CursAcademicCreateParms parm)
            =>
            Task
            .CompletedTask;

        protected override Task<models.CursAcademic> InitializeModel(CursAcademicCreateParms parm)
            =>
            Task.FromResult(
                new models.CursAcademic()
                {
                    AnyInici = parm.AnyInici,
                    EsActiu = parm.EsActiu,
                    Nom = $"{parm.AnyInici}-{parm.AnyInici+1}"
                }
            );

        protected override async Task PostInitialize(CursAcademic model, CursAcademicCreateParms parm)
        {
            // Només pot haver un curs actual.
            if (!parm.EsActiu)
                return;

            await 
                GetContext()
                .CursosAcademics
                .Where(c=>c!=model)
                .ForEachAsync(c => c.EsActiu = false);

        }

        protected override dtoo.CursAcademic ToDto(models.CursAcademic parm)
            =>
            project
            .CursAcademic
            .ToDto(parm);

    }
}

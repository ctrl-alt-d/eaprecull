using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Threading.Tasks;
using DataModels.Models;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace BusinessLayer.Services
{
    public class CursAcademicCreate :
        BLCreate<Models.CursAcademic, Parms.CursAcademicCreateParms, Dtoo.CursAcademic>,
        ICursAcademicCreate
    {
        public CursAcademicCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(CursAcademicCreateParms parm)
            =>
            new RuleChecker<CursAcademicCreateParms>(parm)
            .AddCheck(RuleHiHaValorsNoInformats, "Comprova que tots els valors estiguin ben informats")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Curs Academic amb aquest mateix any inici")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(CursAcademicCreateParms parm)
            =>
            parm.AnyInici < 1980
            ;

        protected virtual Task<bool> RuleEstaRepetit(CursAcademicCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.AnyInici == parm.AnyInici);

        protected override Task<Models.CursAcademic> InitializeModel(CursAcademicCreateParms parm)
            =>
            Task.FromResult(
                new Models.CursAcademic()
                {
                    AnyInici = parm.AnyInici,
                    EsActiu = parm.EsActiu,
                    Nom = $"{parm.AnyInici}-{parm.AnyInici + 1}"
                }
            );

        protected override Task PostAdd(CursAcademic model, CursAcademicCreateParms parm)
            =>
            NomesUnCursPotEstarMarcatComAcursActual(model, parm);

        protected virtual async Task NomesUnCursPotEstarMarcatComAcursActual(CursAcademic model, CursAcademicCreateParms parm)
        {
            if (!parm.EsActiu)
                return;

            await
                GetContext()
                .CursosAcademics
                .Where(c => c != model)
                .ForEachAsync(c => c.EsActiu = false);
        }

        protected override Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            Project
            .CursAcademic
            .ToDto;

    }
}

using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using System.Linq.Expressions;
using System;
using DataModels.Models;

namespace BusinessLayer.Services
{
    public class CursAcademicUpdate :
        BLUpdate<Models.CursAcademic, Parms.CursAcademicUpdateParms, Dtoo.CursAcademic>,
        ICursAcademicUpdate
    {
        protected override Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            Project
            .CursAcademic
            .ToDto;

        public CursAcademicUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(Models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(Models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            new RuleChecker<Models.CursAcademic, CursAcademicUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "Comprova que tots els valors estiguin ben informats")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Curs Academic amb aquest mateix any inici")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(Models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            parm.AnyInici < 1980
            ;

        protected virtual Task<bool> RuleEstaRepetit(Models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.AnyInici == parm.AnyInici);



        protected override Task UpdateModel(Models.CursAcademic model, CursAcademicUpdateParms parm)
        {
            model.AnyInici = parm.AnyInici;
            model.EsActiu = parm.EsActiu;
            model.Nom = $"{parm.AnyInici}-{parm.AnyInici + 1}";
            return Task.CompletedTask;
        }

        protected override void ResetReferences(CursAcademic model)
        { }
    }
}

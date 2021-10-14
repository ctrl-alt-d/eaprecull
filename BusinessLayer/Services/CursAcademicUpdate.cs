using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using System.Linq.Expressions;
using System;

namespace BusinessLayer.Services
{
    public class CursAcademicUpdate :
        BLUpdate<models.CursAcademic, parms.CursAcademicUpdateParms, dtoo.CursAcademic>,
        ICursAcademicUpdate
    {
        protected override Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto
            =>
            project
            .CursAcademic
            .ToDto;

        public CursAcademicUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            new RuleChecker<models.CursAcademic, CursAcademicUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "Comprova que tots els valors estiguin ben informats")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Curs Academic amb aquest mateix any inici")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            parm.AnyInici < 1980
            ;

        protected virtual Task<bool> RuleEstaRepetit(models.CursAcademic model, CursAcademicUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.AnyInici == parm.AnyInici);



        protected override Task UpdateModel(models.CursAcademic model, CursAcademicUpdateParms parm)
        {
            model.AnyInici = parm.AnyInici;
            model.EsActiu = parm.EsActiu;
            model.Nom = $"{parm.AnyInici}-{parm.AnyInici + 1}";
            return Task.CompletedTask;
        }
    }
}

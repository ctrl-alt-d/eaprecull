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
using DataModels.Models;

namespace BusinessLayer.Services
{
    public class TipusActuacioUpdate :
        BLUpdate<models.TipusActuacio, parms.TipusActuacioUpdateParms, dtoo.TipusActuacio>,
        ITipusActuacioUpdate
    {
        protected override Expression<Func<models.TipusActuacio, dtoo.TipusActuacio>> ToDto
            =>
            project
            .TipusActuacio
            .ToDto;

        public TipusActuacioUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            new RuleChecker<models.TipusActuacio, TipusActuacioUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre TipusActuacio amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);



        protected override Task UpdateModel(models.TipusActuacio model, TipusActuacioUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }

        protected override void ResetReferences(TipusActuacio model)
        {}

    }
}

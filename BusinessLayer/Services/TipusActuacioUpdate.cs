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
    public class TipusActuacioUpdate :
        BLUpdate<Models.TipusActuacio, Parms.TipusActuacioUpdateParms, Dtoo.TipusActuacio>,
        ITipusActuacioUpdate
    {
        protected override Expression<Func<Models.TipusActuacio, Dtoo.TipusActuacio>> ToDto
            =>
            Project
            .TipusActuacio
            .ToDto;

        public TipusActuacioUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(Models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(Models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            new RuleChecker<Models.TipusActuacio, TipusActuacioUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre TipusActuacio amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(Models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(Models.TipusActuacio model, TipusActuacioUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);



        protected override Task UpdateModel(Models.TipusActuacio model, TipusActuacioUpdateParms parm)
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

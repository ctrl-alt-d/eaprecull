using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using DataModels.Models;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using System.Linq.Expressions;
using System;

namespace BusinessLayer.Services
{
    public class TipusActuacioCreate :
        BLCreate<models.TipusActuacio, parms.TipusActuacioCreateParms, dtoo.TipusActuacio>,
        ITipusActuacioCreate
    {
        public TipusActuacioCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(TipusActuacioCreateParms parm)
            =>
            new RuleChecker<TipusActuacioCreateParms>(parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre TipusActuacio amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(TipusActuacioCreateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(TipusActuacioCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Task<models.TipusActuacio> InitializeModel(TipusActuacioCreateParms parm)
            =>
            Task
            .FromResult(
                new models.TipusActuacio()
                {
                    Codi = parm.Codi,
                    Nom = parm.Nom,
                    EsActiu = parm.EsActiu,
                }
            );

        protected override Task PostAdd(TipusActuacio model, TipusActuacioCreateParms parm)
            =>
            Task
            .CompletedTask;

        protected override Expression<Func<models.TipusActuacio, dtoo.TipusActuacio>> ToDto
            =>
            project
            .TipusActuacio
            .ToDto;

    }
}

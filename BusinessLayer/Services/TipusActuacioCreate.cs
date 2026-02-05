using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
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
        BLCreate<Models.TipusActuacio, Parms.TipusActuacioCreateParms, Dtoo.TipusActuacio>,
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

        protected override Task<Models.TipusActuacio> InitializeModel(TipusActuacioCreateParms parm)
            =>
            Task
            .FromResult(
                new Models.TipusActuacio()
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

        protected override Expression<Func<Models.TipusActuacio, Dtoo.TipusActuacio>> ToDto
            =>
            Project
            .TipusActuacio
            .ToDto;

    }
}

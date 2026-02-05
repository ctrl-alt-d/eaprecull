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
    public class EtapaCreate :
        BLCreate<Models.Etapa, Parms.EtapaCreateParms, Dtoo.Etapa>,
        IEtapaCreate
    {
        public EtapaCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(EtapaCreateParms parm)
            =>
            new RuleChecker<EtapaCreateParms>(parm)
            .AddCheck(RuleHiHaValorsNoInformats, "Comprova que tens totes les dades informades")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Etapa amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(EtapaCreateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom) ||
            string.IsNullOrEmpty(parm.Codi)
            ;

        protected virtual Task<bool> RuleEstaRepetit(EtapaCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Task<Models.Etapa> InitializeModel(EtapaCreateParms parm)
            =>
            Task
            .FromResult(
                new Models.Etapa()
                {
                    Codi = parm.Codi,
                    Nom = parm.Nom,
                    SonEstudisObligatoris = parm.SonEstudisObligatoris,
                    EsActiu = parm.EsActiu,
                }
            );

        protected override Task PostAdd(Etapa model, EtapaCreateParms parm)
            =>
            Task
            .CompletedTask;

        protected override Expression<Func<Models.Etapa, Dtoo.Etapa>> ToDto
            =>
            Project
            .Etapa
            .ToDto;

    }
}

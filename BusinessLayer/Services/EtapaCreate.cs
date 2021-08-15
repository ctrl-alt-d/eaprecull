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
    public class EtapaCreate : 
        BLCreate<models.Etapa, parms.EtapaCreateParms, dtoo.Etapa>,
        IEtapaCreate
    {
        public EtapaCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(EtapaCreateParms parm)
            =>
            new RuleChecker<EtapaCreateParms>(parm)
            .AddCheck( RuleValorsEstanInformats, "Comprova que tens totes les dades informades" )
            .AddCheck( RuleNoEstaRepetit, "Ja existeix un altre Etapa amb aquest mateix nom o codi" )
            .Check();

        protected virtual bool RuleValorsEstanInformats(EtapaCreateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom) || 
            string.IsNullOrEmpty(parm.Codi)
            ;

        protected virtual Task<bool> RuleNoEstaRepetit(EtapaCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Task<models.Etapa> InitializeModel(EtapaCreateParms parm)
            =>
            Task
            .FromResult(
                new models.Etapa()
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

        protected override Expression<Func<models.Etapa, dtoo.Etapa>> ToDto
            =>
            project
            .Etapa
            .ToDto;

    }
}

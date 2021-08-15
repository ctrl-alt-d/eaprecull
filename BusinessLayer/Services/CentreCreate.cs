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
    public class CentreCreate : 
        BLCreate<models.Centre, parms.CentreCreateParms, dtoo.Centre>,
        ICentreCreate
    {
        public CentreCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(CentreCreateParms parm)
            =>
            new RuleChecker<CentreCreateParms>(parm)
            .AddCheck( RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc" )
            .AddCheck( RuleEstaRepetit, "Ja existeix un altre centre amb aquest mateix nom o codi" )
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(CentreCreateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(CentreCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Task<models.Centre> InitializeModel(CentreCreateParms parm)
            =>
            Task
            .FromResult(
                new models.Centre()
                {
                    Codi = parm.Codi,
                    Nom = parm.Nom,
                    EsActiu = parm.EsActiu,
                }
            );

        protected override Task PostAdd(Centre model, CentreCreateParms parm)
            =>
            Task
            .CompletedTask;

        protected override Expression<Func<models.Centre, dtoo.Centre>> ToDto
            =>
            project
            .Centre
            .ToDto;

    }
}

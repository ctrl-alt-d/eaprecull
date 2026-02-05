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
    public class CentreCreate :
        BLCreate<Models.Centre, Parms.CentreCreateParms, Dtoo.Centre>,
        ICentreCreate
    {
        public CentreCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(CentreCreateParms parm)
            =>
            new RuleChecker<CentreCreateParms>(parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre centre amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(CentreCreateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(CentreCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Task<Models.Centre> InitializeModel(CentreCreateParms parm)
            =>
            Task
            .FromResult(
                new Models.Centre()
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

        protected override Expression<Func<Models.Centre, Dtoo.Centre>> ToDto
            =>
            Project
            .Centre
            .ToDto;

    }
}

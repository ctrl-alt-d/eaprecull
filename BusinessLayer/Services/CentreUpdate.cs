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
    public class CentreUpdate :
        BLUpdate<Models.Centre, Parms.CentreUpdateParms, Dtoo.Centre>,
        ICentreUpdate
    {
        protected override Expression<Func<Models.Centre, Dtoo.Centre>> ToDto
            =>
            Project
            .Centre
            .ToDto;

        public CentreUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(Models.Centre model, CentreUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(Models.Centre model, CentreUpdateParms parm)
            =>
            new RuleChecker<Models.Centre, CentreUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre centre amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(Models.Centre model, CentreUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(Models.Centre model, CentreUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);



        protected override Task UpdateModel(Models.Centre model, CentreUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }

        protected override void ResetReferences(Centre model)
        { }
    }
}

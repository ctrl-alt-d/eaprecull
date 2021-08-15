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
    public class CentreUpdate :
        BLUpdate<models.Centre, parms.CentreUpdateParms, dtoo.Centre>,
        ICentreUpdate
    {
        protected override Expression<Func<models.Centre, dtoo.Centre>> ToDto 
            =>
            project
            .Centre
            .ToDto;

        public CentreUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.Centre model, CentreUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.Centre model, CentreUpdateParms parm)
            =>
            new RuleChecker<models.Centre, CentreUpdateParms>(model, parm)
            .AddCheck( RuleValorsEstanInformats, "No es pot deixar el Nom en blanc" )
            .AddCheck( RuleNoEstaRepetit, "Ja existeix un altre centre amb aquest mateix nom o codi" )
            .Check();

        protected virtual bool RuleValorsEstanInformats(models.Centre model, CentreUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleNoEstaRepetit(models.Centre model, CentreUpdateParms parm)
            =>
            GetCollection()
            .Where(x=> x.Id != model.Id)
            .AnyAsync(x=> x.Codi == parm.Codi || x.Nom == parm.Nom);

            

        protected override Task UpdateModel(models.Centre model, CentreUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }
    }
}

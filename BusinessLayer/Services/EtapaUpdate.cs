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
    public class EtapaUpdate :
        BLUpdate<Models.Etapa, Parms.EtapaUpdateParms, Dtoo.Etapa>,
        IEtapaUpdate
    {
        public EtapaUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(Models.Etapa model, EtapaUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(Models.Etapa model, EtapaUpdateParms parm)
            =>
            new RuleChecker<Models.Etapa, EtapaUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Etapa amb aquest mateix nom o codi")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(Models.Etapa model, EtapaUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom) ||
            string.IsNullOrEmpty(parm.Codi);

        protected virtual Task<bool> RuleEstaRepetit(Models.Etapa model, EtapaUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override Expression<Func<Models.Etapa, Dtoo.Etapa>> ToDto
            =>
            Project
            .Etapa
            .ToDto;

        protected override Task UpdateModel(Models.Etapa model, EtapaUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }

        protected override void ResetReferences(Etapa model)
        {}
    }
}

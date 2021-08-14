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

namespace BusinessLayer.Services
{
    public class EtapaUpdate :
        BLUpdate<models.Etapa, parms.EtapaUpdateParms, dtoo.Etapa>,
        IEtapaUpdate
    {
        public EtapaUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.Etapa model, EtapaUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.Etapa model, EtapaUpdateParms parm)
            =>
            new RuleChecker<models.Etapa, EtapaUpdateParms>(model, parm)
            .AddCheck( RuleValorsEstanInformats, "No es pot deixar el Nom en blanc" )
            .AddCheck( RuleNoEstaRepetit, "Ja existeix un altre Etapa amb aquest mateix nom o codi" )
            .Check();

        protected virtual bool RuleValorsEstanInformats(models.Etapa model, EtapaUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom) || 
            string.IsNullOrEmpty(parm.Codi);

        protected virtual Task<bool> RuleNoEstaRepetit(models.Etapa model, EtapaUpdateParms parm)
            =>
            GetCollection()
            .Where(x=> x.Id != model.Id)
            .AnyAsync(x=> x.Codi == parm.Codi || x.Nom == parm.Nom);

        protected override dtoo.Etapa ToDto(models.Etapa model)
            =>
            project
            .Etapa
            .ToDto(model);

        protected override Task UpdateModel(models.Etapa model, EtapaUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }
    }
}

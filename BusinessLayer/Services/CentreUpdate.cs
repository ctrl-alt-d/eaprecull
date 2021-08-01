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

namespace BusinessLayer.Services
{
    public class CentreUpdate :
        BLUpdate<models.Centre, parms.CentreUpdateParms, dtoo.Centre>,
        ICentreUpdate
    {
        public CentreUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.Centre model, CentreUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.Centre model, CentreUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override dtoo.Centre ToDto(models.Centre parm)
        {
            throw new System.NotImplementedException();
        }

        protected override Task UpdateModel(models.Centre model, CentreUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }
    }
}

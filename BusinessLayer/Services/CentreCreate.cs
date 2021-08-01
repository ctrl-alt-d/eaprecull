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
    public class CentreCreate : 
        BLCreate<models.Centre, parms.CentreCreateParms, dtoo.Centre>,
        ICentreCreate
    {
        public CentreCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(CentreCreateParms parm) 
            =>
            Task
            .CompletedTask;

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

        protected override Task PostInitialize(Centre model, CentreCreateParms parm)
            =>
            Task
            .CompletedTask;

        protected override dtoo.Centre ToDto(models.Centre parm)
            =>
            project
            .Centre
            .ToDto(parm);

    }
}

using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;

namespace BusinessLayer.Services
{
    public class CentreCreate : 
        BLCreate<models.Centre, parms.CentreCreateParms, dtoo.Centre>,
        ICentreCreate
    {
        public CentreCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override models.Centre SetValues(CentreCreateParms parm)
            =>
            new()
            {
                Codi = parm.Codi,
                Nom = parm.Nom,
                EsActiu = parm.EsActiu,
            };
        

        protected override dtoo.Centre ToDto(models.Centre parm)
            =>
            project
            .Centre
            .ToDto(parm);
    }
}

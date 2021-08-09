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
    public class CentreGet : 
        BLGet<models.Centre, dtoo.Centre>,
        ICentreGet
    {
        public CentreGet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override dtoo.Centre ToDto(models.Centre model)
            =>
            project
            .Centre
            .ToDto(model);

    }
}

using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using project = DTO.Projections;
using models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;

namespace BusinessLayer.Services
{
    public class Centres :
        BLGetItems<models.Centre, parms.EsActiuParms, dtoo.Centre>,
        ICentres
    {
        public Centres(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Centre> GetModels(parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c=>c.Nom);

        protected override dtoo.Centre ToDto(models.Centre model)
            =>
            project
            .Centre
            .ToDto(model);

    }
}

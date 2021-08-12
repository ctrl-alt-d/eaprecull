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

        protected override async Task PreUpdate(models.Centre model, CentreUpdateParms parm)
        {
            // Està repetit?
            var repetit =
                await
                GetCollection()
                .Where(x=> x.Id != model.Id)
                .AnyAsync(x=> x.Codi == parm.Codi || x.Nom == parm.Nom);

            if (repetit)
                throw new BrokenRuleException("Ja existeix un altre centre amb aquest mateix nom o codi");

            // El nom és correcte?
            if (string.IsNullOrEmpty(parm.Nom))
                throw new BrokenRuleException("No es pot deixar el Nom en blanc");

        }
            

        protected override dtoo.Centre ToDto(models.Centre model)
            =>
            project
            .Centre
            .ToDto(model);

        protected override Task UpdateModel(models.Centre model, CentreUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.EsActiu = parm.EsActiu;
            model.Nom = parm.Nom;
            return Task.CompletedTask;
        }
    }
}

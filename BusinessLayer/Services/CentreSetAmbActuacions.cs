using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using dtoo = DTO.o.DTOs;
using System.Linq;
using models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DTO.i.DTOs;

namespace BusinessLayer.Services
{
    public class CentreSetAmbActuacions :
        CentreSet,
        ICentreSetAmbActuacions
    {
        public CentreSetAmbActuacions(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.Centre, dtoo.Centre>> ToDto
        {
            get
            {
                var ctx = GetContext();
                var cursActiuId = ctx.Set<models.CursAcademic>()
                    .Where(c => c.EsActiu)
                    .Select(c => c.Id)
                    .FirstOrDefault();

                return model => new dtoo.CentreAmbActuacions(
                    model.Id,
                    model.Codi,
                    model.Nom,
                    model.EsActiu,
                    model.Etiqueta,
                    model.Descripcio,
                    ctx.Set<models.Actuacio>().Count(a => a.CentreAlMomentDeLactuacio.Id == model.Id),
                    ctx.Set<models.Actuacio>().Count(a => a.CentreAlMomentDeLactuacio.Id == model.Id && a.CursActuacio.Id == cursActiuId)
                );
            }
        }
    }
}

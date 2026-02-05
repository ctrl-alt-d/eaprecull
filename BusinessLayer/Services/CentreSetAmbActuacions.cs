using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using Dtoo = DTO.o.DTOs;
using System.Linq;
using Models = DataModels.Models;
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

        protected override Expression<Func<Models.Centre, Dtoo.Centre>> ToDto
        {
            get
            {
                var ctx = GetContext();
                var cursActiuId = ctx.Set<Models.CursAcademic>()
                    .Where(c => c.EsActiu)
                    .Select(c => c.Id)
                    .FirstOrDefault();

                return model => new Dtoo.CentreAmbActuacions(
                    model.Id,
                    model.Codi,
                    model.Nom,
                    model.EsActiu,
                    model.Etiqueta,
                    model.Descripcio,
                    ctx.Set<Models.Actuacio>().Count(a => a.CentreAlMomentDeLactuacio.Id == model.Id),
                    ctx.Set<Models.Actuacio>().Count(a => a.CentreAlMomentDeLactuacio.Id == model.Id && a.CursActuacio.Id == cursActiuId)
                );
            }
        }
    }
}

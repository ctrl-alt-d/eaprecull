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
using System.Linq.Expressions;
using System;
using DataModels.Models;

namespace BusinessLayer.Services
{
    public class ActuacioUpdate :
        BLUpdate<models.Actuacio, parms.ActuacioUpdateParms, dtoo.Actuacio>,
        IActuacioUpdate
    {
        protected override Expression<Func<models.Actuacio, dtoo.Actuacio>> ToDto 
            =>
            project
            .Actuacio
            .ToDto;

        public ActuacioUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.Actuacio model, ActuacioUpdateParms parm)
        {
            AlumnePrevi.NombreTotalDactuacions --;
            model.Alumne.NombreTotalDactuacions++;
            return Task.CompletedTask;
        }
            

        protected override async Task PreUpdate(models.Actuacio model, ActuacioUpdateParms parm)
        {
            await GetContext().Entry(model).Reference(m=>m.Alumne).LoadAsync();
            AlumnePrevi = model.Alumne;
        }

        protected override async Task UpdateModel(models.Actuacio model, ActuacioUpdateParms parm)
        =>
            model.SetMainData(
                alumne: await Perfection<Alumne>(parm.AlumneId),
                tipusActuacio: await Perfection<TipusActuacio>(parm.TipusActuacioId),
                observacionsTipusActuacio: parm.ObservacionsTipusActuacio,
                momentDeLactuacio: parm.MomentDeLactuacio,
                cursActuacio: await Perfection<CursAcademic>(parm.CursActuacioId),
                centreAlMomentDeLactuacio: await Perfection<Centre>(parm.CentreAlMomentDeLactuacioId),
                etapaAlMomentDeLactuacio:  await Perfection<Etapa>(parm.EtapaAlMomentDeLactuacioId),
                nivellAlMomentDeLactuacio: parm.NivellAlMomentDeLactuacio,
                minutsDuradaActuacio: parm.MinutsDuradaActuacio,
                descripcioActuacio: parm.DescripcioActuacio
            );

        protected Alumne AlumnePrevi {get; set; } = default!;
    }
}

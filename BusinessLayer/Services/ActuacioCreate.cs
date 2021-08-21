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
using System.Linq.Expressions;
using System;
using System.Linq;

namespace BusinessLayer.Services
{
    public class ActuacioCreate :
        BLCreate<models.Actuacio, parms.ActuacioCreateParms, dtoo.Actuacio>,
        IActuacioCreate
    {
        protected override Expression<Func<Actuacio, dtoo.Actuacio>> ToDto
            =>
            project
            .Actuacio
            .ToDto;

        public ActuacioCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(ActuacioCreateParms parm)
            =>
            Task.CompletedTask;


        protected override async Task<models.Actuacio> InitializeModel(ActuacioCreateParms parm)
            =>
            new(
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

        protected override Task PostAdd(Actuacio model, ActuacioCreateParms parm)
        {
            model.Alumne.NombreTotalDactuacions++;
            return Task.CompletedTask;
        }

    }
}

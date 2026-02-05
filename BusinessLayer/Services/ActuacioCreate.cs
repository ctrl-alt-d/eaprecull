using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
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
        BLCreate<Models.Actuacio, Parms.ActuacioCreateParms, Dtoo.Actuacio>,
        IActuacioCreate
    {
        protected override Expression<Func<Actuacio, Dtoo.Actuacio>> ToDto
            =>
            Project
            .Actuacio
            .ToDto;

        public ActuacioCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(ActuacioCreateParms parm)
            =>
            Task.CompletedTask;


        protected override async Task<Models.Actuacio> InitializeModel(ActuacioCreateParms parm)
            =>
            new(
                alumne: await Perfection<Alumne>(parm.AlumneId),
                tipusActuacio: await Perfection<TipusActuacio>(parm.TipusActuacioId),
                observacionsTipusActuacio: parm.ObservacionsTipusActuacio,
                momentDeLactuacio: parm.MomentDeLactuacio,
                cursActuacio: await Perfection<CursAcademic>(parm.CursActuacioId),
                centreAlMomentDeLactuacio: await Perfection<Centre>(parm.CentreAlMomentDeLactuacioId),
                etapaAlMomentDeLactuacio: await Perfection<Etapa>(parm.EtapaAlMomentDeLactuacioId),
                nivellAlMomentDeLactuacio: parm.NivellAlMomentDeLactuacio,
                minutsDuradaActuacio: parm.MinutsDuradaActuacio,
                descripcioActuacio: parm.DescripcioActuacio
            );

        protected override async Task PostAdd(Actuacio model, ActuacioCreateParms parm)
        {
            // Recuperar alumne
            await LoadReference(model, m => m.Alumne);

            // incrementar número actuacions
            model.Alumne.NombreTotalDactuacions++;

            // "touch" per tal que aparegui el primer
            model.Alumne.DataDarreraModificacio = DateTime.Now;
        }

    }
}

using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
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
        BLUpdate<Models.Actuacio, Parms.ActuacioUpdateParms, Dtoo.Actuacio>,
        IActuacioUpdate
    {
        protected override Expression<Func<Models.Actuacio, Dtoo.Actuacio>> ToDto
            =>
            Project
            .Actuacio
            .ToDto;

        public ActuacioUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected Alumne AlumnePrevi { get; set; } = default!;
        protected override async Task PreUpdate(Models.Actuacio model, ActuacioUpdateParms parm)
        {
            await LoadReference(model, m => m.Alumne);
            AlumnePrevi = model.Alumne;
        }

        protected override void ResetReferences(Actuacio model)
            =>
            ReferencesAreModify(
                model,
                x => x.Alumne,
                x => x.TipusActuacio,
                x => x.CursActuacio,
                x => x.CentreAlMomentDeLactuacio,
                x => x.EtapaAlMomentDeLactuacio
            );

        protected override async Task UpdateModel(Models.Actuacio model, ActuacioUpdateParms parm)
            =>
            model.SetMainData(
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

        protected override async Task PostUpdate(Models.Actuacio model, ActuacioUpdateParms parm)
        {
            // Decrementar nombre actuacions a l'alumne anterior
            AlumnePrevi.NombreTotalDactuacions--;

            // Recuperar alumne de l'actuació
            await LoadReference(model, m => m.Alumne);

            // incrementar número actuacions a l'alumne actual
            model.Alumne.NombreTotalDactuacions++;

            // "touch" per tal que l'alumne aparegui el primer
            model.Alumne.DataDarreraModificacio = DateTime.Now;
        }

    }
}

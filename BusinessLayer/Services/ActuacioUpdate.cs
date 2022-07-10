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

        protected Alumne AlumnePrevi { get; set; } = default!;
        protected override async Task PreUpdate(models.Actuacio model, ActuacioUpdateParms parm)
        {
            await LoadReference(model, m => m.Alumne);
            AlumnePrevi = model.Alumne;
        }

        protected override void ResetReferences(Actuacio model)
            =>
            ReferencesAreModify(
                model,
                x=>x.Alumne,
                x=>x.TipusActuacio,
                x=>x.CursActuacio,
                x=>x.CentreAlMomentDeLactuacio,
                x=>x.EtapaAlMomentDeLactuacio
            );

        protected override async Task UpdateModel(models.Actuacio model, ActuacioUpdateParms parm)
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

        protected override async Task PostUpdate(models.Actuacio model, ActuacioUpdateParms parm)
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

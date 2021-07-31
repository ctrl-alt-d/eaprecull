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

namespace BusinessLayer.Services
{
    public class AlumneCreate :
        BLCreate<models.Alumne, parms.AlumneCreateParms, dtoo.Alumne>,
        IAlumneCreate
    {
        public AlumneCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override models.Alumne SetValues(AlumneCreateParms parm)
            =>
            new()
            {
                Nom = parm.Nom,
                Cognoms = parm.Cognoms,
                DataNaixement = parm.DataNaixement,
                CentreActual = Perfection<Centre>(parm.CentreActualId),
                CursDarreraActualitacioDades = Perfection<CursAcademic>(parm.CursDarreraActualitacioDadesId),
                EtapaActual = Perfection<Etapa>(parm.EtapaActualId),
                DataInformeNESENEE = parm.DataInformeNESENEE,
                ObservacionsNESENEE = parm.ObservacionsNESENEE,
                DataInformeNESENoNEE = parm.DataInformeNESENoNEE,
                ObservacionsNESENoNEE = parm.ObservacionsNESENoNEE,
            };

        protected override dtoo.Alumne ToDto(models.Alumne parm)
            =>
            project
            .Alumne
            .ToDto(parm);
    }
}

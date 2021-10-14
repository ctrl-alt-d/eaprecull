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
    public class AlumneCreate :
        BLCreate<models.Alumne, parms.AlumneCreateParms, dtoo.Alumne>,
        IAlumneCreate
    {
        protected override Expression<Func<Alumne, dtoo.Alumne>> ToDto
            =>
            project
            .Alumne
            .ToDto;

        public AlumneCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PreInitialize(AlumneCreateParms parm)
            =>
            new RuleChecker<AlumneCreateParms>(parm)
            .AddCheck(RuleNoHiHaCapCursActiu, "Abans de crear cap alumne cal que hi hagi un curs marcat com actiu.")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Alumne amb aquest mateix nom, cognoms i data de naixement")
            .Check();

        protected virtual async Task<bool> RuleNoHiHaCapCursActiu(AlumneCreateParms _)
            =>
            !
            await (
                GetContext()
                .CursosAcademics
                .Where(x => x.EsActiu)
                .AnyAsync()
            );

        protected virtual Task<bool> RuleEstaRepetit(AlumneCreateParms parm)
            =>
            GetCollection()
            .AnyAsync(x => x.Cognoms == parm.Cognoms && x.Nom == parm.Nom && x.DataNaixement == parm.DataNaixement);

        protected override async Task<models.Alumne> InitializeModel(AlumneCreateParms parm)
            =>
            new()
            {
                Nom = parm.Nom,
                Cognoms = parm.Cognoms,
                DataNaixement = parm.DataNaixement,
                CentreActual = await Perfection<Centre>(parm.CentreActualId),
                CursDarreraActualitacioDades = await Perfection<CursAcademic>(parm.CursDarreraActualitacioDadesId),
                EtapaActual = await Perfection<Etapa>(parm.EtapaActualId),
                NivellActual = parm.NivellActual,
                DataInformeNESENEE = parm.DataInformeNESENEE,
                ObservacionsNESENEE = parm.ObservacionsNESENEE,
                DataInformeNESENoNEE = parm.DataInformeNESENoNEE,
                ObservacionsNESENoNEE = parm.ObservacionsNESENoNEE,
                Tags = parm.Tags,
                EsActiu = true,
                DataDarreraModificacio = DateTime.Now,
            };

        protected override Task PostAdd(Alumne model, AlumneCreateParms parm)
            =>
            Task
            .CompletedTask;



    }
}

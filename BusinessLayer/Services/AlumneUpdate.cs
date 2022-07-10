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
    public class AlumneUpdate :
        BLUpdate<models.Alumne, parms.AlumneUpdateParms, dtoo.Alumne>,
        IAlumneUpdate
    {
        protected override Expression<Func<models.Alumne, dtoo.Alumne>> ToDto
            =>
            project
            .Alumne
            .ToDto;

        public AlumneUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Task PostUpdate(models.Alumne model, AlumneUpdateParms parm)
            =>
            Task.CompletedTask;

        protected override Task PreUpdate(models.Alumne model, AlumneUpdateParms parm)
            =>
            new RuleChecker<models.Alumne, AlumneUpdateParms>(model, parm)
            .AddCheck(RuleHiHaValorsNoInformats, "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix un altre Alumne amb aquest mateix nom, cognoms i data de naixement")
            .AddCheck(RuleNoHiHaCapCursActiu, "Abans de crear cap alumne cal que hi hagi un curs marcat com actiu.")
            .Check();

        protected virtual bool RuleHiHaValorsNoInformats(models.Alumne model, AlumneUpdateParms parm)
            =>
            string.IsNullOrEmpty(parm.Nom);

        protected virtual Task<bool> RuleEstaRepetit(models.Alumne model, AlumneUpdateParms parm)
            =>
            GetCollection()
            .Where(x => x.Id != model.Id)
            .AnyAsync(x => x.Cognoms == parm.Cognoms && x.Nom == parm.Nom && x.DataNaixement == parm.DataNaixement);

        protected virtual async Task<bool> RuleNoHiHaCapCursActiu(models.Alumne m, AlumneUpdateParms p)
            =>
            !
            await (
                GetContext()
                .CursosAcademics
                .Where(x => x.EsActiu)
                .AnyAsync()
            );

        protected override async Task UpdateModel(models.Alumne model, AlumneUpdateParms parm)
        {
            model.Nom = parm.Nom;
            model.Cognoms = parm.Cognoms;
            model.DataNaixement = parm.DataNaixement;
            model.CentreActual = await Perfection<Centre>(parm.CentreActualId);
            model.CursDarreraActualitacioDades = await Perfection<CursAcademic>(parm.CursDarreraActualitacioDadesId);
            model.EtapaActual = await Perfection<Etapa>(parm.EtapaActualId);
            model.NivellActual = parm.NivellActual;
            model.DataInformeNESENEE = parm.DataInformeNESENEE;
            model.ObservacionsNESENEE = parm.ObservacionsNESENEE;
            model.DataInformeNESENoNEE = parm.DataInformeNESENoNEE;
            model.ObservacionsNESENoNEE = parm.ObservacionsNESENoNEE;
            model.EsActiu = parm.EsActiu;
            model.Tags = parm.Tags;
            model.DataDarreraModificacio = DateTime.Now;
        }

        protected override Task LoadUpdatableReferences(models.Alumne model)
            =>
            LoadReferences(
                model,
                x=>x.CentreActual,
                x=>x.CursDarreraActualitacioDades,
                x=>x.EtapaActual);
    }
}

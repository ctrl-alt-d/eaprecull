using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using project = DTO.Projections;
using models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System;
using DataModels.Models;
using DTO.i.DTOs;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class ActuacioSet :
        BLSet<models.Actuacio, parms.ActuacioSearchParms, dtoo.Actuacio>,
        IActuacioSet
    {
        public ActuacioSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Actuacio> GetModels(parms.ActuacioSearchParms request)
        {
            var query = GetAllModels();

            //
            query = MatchSearchString(query, request);

            //
            query = MatchCentre(query, request);
            query = MatchAlumne(query, request);
            query = MatchTipusActuacio(query, request);
            query = MatchCursActuacio(query, request);
            query = MatchAlumneEsActiu(query, request);
            query = MatchMatchDataActuacioAnteriorA(query, request);
            query = MatchDataActuacioPosteriorA(query, request);

            query = query.OrderByDescending(x => x.MomentDeLactuacio);

            return query;
        }



        private IQueryable<Actuacio> MatchCentre(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.CentreId.HasValue)
                return query;

            return query.Where(model =>
                model.Alumne.CentreActual != null &&
                model.Alumne.CentreActual.Id == request.CentreId);
        }

        private IQueryable<Actuacio> MatchMatchDataActuacioAnteriorA(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.AmbDataActuacioAnteriorA.HasValue)
                return query;

            return query.Where(model =>
                model.MomentDeLactuacio <= request.AmbDataActuacioAnteriorA);
        }

        private IQueryable<Actuacio> MatchDataActuacioPosteriorA(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.AmbDataActuacioAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.MomentDeLactuacio >= request.AmbDataActuacioAnteriorA);
        }
        private IQueryable<Actuacio> MatchTipusActuacio(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.TipusActuacioId.HasValue)
                return query;

            return query.Where(model => model.TipusActuacio.Id == request.TipusActuacioId);
        }

        private IQueryable<Actuacio> MatchAlumne(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.AlumneId.HasValue)
                return query;

            return query.Where(model => model.Alumne.Id == request.AlumneId);
        }
        private IQueryable<Actuacio> MatchCursActuacio(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.CursActuacioId.HasValue)
                return query;

            return query.Where(model => model.CursActuacio.Id == request.CursActuacioId);
        }

        private IQueryable<Actuacio> MatchAlumneEsActiu(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (!request.AlumneEsActiu.HasValue ) 
                return query;

            return query.Where(model => model.Alumne.EsActiu);
        }
       

        private IQueryable<Actuacio> MatchSearchString(IQueryable<Actuacio> query, ActuacioSearchParms request)
        {
            if (string.IsNullOrWhiteSpace(request.SearchString))
                return query;

            var tokens = request.SearchString.Split().Select(x => x.Trim()).ToList();

            // SearchString: Alumne (nom, cognoms i tag), Centre, Descripcio, Tipus act, Curs act.
            tokens.ForEach(token =>
                query = query.Where(model => (

                    // alumne nom, cognoms i tags
                    EF.Functions.Like(model.Alumne.Nom, $"%{token}%") ||
                    EF.Functions.Like(model.Alumne.Cognoms, $"%{token}%") ||
                    EF.Functions.Like(model.Alumne.Tags, $"%{token}%") ||

                    // centre
                    (model.Alumne.CentreActual != null && EF.Functions.Like(model.Alumne.CentreActual.Nom, $"%{token}%"))||
                    (model.Alumne.CentreActual != null && EF.Functions.Like(model.Alumne.CentreActual.Codi, $"%{token}%"))||

                    // descripcions
                    EF.Functions.Like(model.ObservacionsTipusActuacio, $"%{token}%") ||
                    EF.Functions.Like(model.DescripcioActuacio, $"%{token}%") ||

                    // tipus actuacio
                    EF.Functions.Like(model.TipusActuacio.Nom, $"%{token}%") ||
                    EF.Functions.Like(model.TipusActuacio.Codi, $"%{token}%") ||

                    // curs actuacio
                    EF.Functions.Like(model.CursActuacio.Nom, $"%{token}%")
            )));
            return query;
        }

        //
        protected override Expression<Func<models.Actuacio, dtoo.Actuacio>> ToDto
            =>
            project
            .Actuacio
            .ToDto;

    }
}

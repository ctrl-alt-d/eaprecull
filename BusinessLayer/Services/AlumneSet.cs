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
    public class AlumneSet :
        BLSet<models.Alumne, parms.AlumneSearchParms, dtoo.Alumne>,
        IAlumneSet
    {
        public AlumneSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Alumne> GetModels(parms.AlumneSearchParms request)
        {
            var query = GetAllModels();
            query = MatchCentre(query, request);
            query = MatchDarreraModificacioAnteriorA(query, request);
            query = MatchDarreraModificacioPosteriorA(query, request);
            query = MatchDataNaixementAnteriorA(query, request);
            query = MatchDataNaixementPosteriorA(query, request);
            query = MatchDataInformeAnteriorA(query, request);
            query = MatchDataInformePosteriorA(query, request);
            query = MatchEtapa(query, request);
            query = MatchTipusActuacio(query, request);
            query = MatchNomCognomsCentre(query, request);
            query = MatchTags(query, request);
            query = MatchEsActiu(query, request);
            query = Ordena(query, request.OrdreResultats);

            return query;
        }

        private IQueryable<Alumne> Ordena(IQueryable<Alumne> alumnes, AlumneSearchParms.OrdreResultatsChoice ordreResultats)
            =>
            ordreResultats switch
            {
                AlumneSearchParms.OrdreResultatsChoice.CognomsNom =>
                    alumnes
                    .OrderBy(x => x.Cognoms)
                    .ThenBy(x => x.Nom),
                AlumneSearchParms.OrdreResultatsChoice.DarreraModificacio =>
                    alumnes
                    .OrderBy(x => x.DataDarreraModificacio),
                _ => alumnes
            };


        private IQueryable<Alumne> MatchCentre(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.CentreId.HasValue)
                return query;

            return query.Where(model =>
                model.CentreActual != null &&
                model.CentreActual.Id == request.CentreId);
        }

        private IQueryable<Alumne> MatchDarreraModificacioAnteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDarreraModificacioAnteriorA.HasValue)
                return query;

            return query.Where(model =>
                model.DataDarreraModificacio <= request.AmbDarreraModificacioAnteriorA);
        }

        private IQueryable<Alumne> MatchDarreraModificacioPosteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDarreraModificacioAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.DataDarreraModificacio >= request.AmbDarreraModificacioAnteriorA);
        }

        //
        private IQueryable<Alumne> MatchDataNaixementAnteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDataNaixementAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.DataNaixement <= request.AmbDataNaixementAnteriorA);
        }

        private IQueryable<Alumne> MatchDataNaixementPosteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDataNaixementAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.DataNaixement >= request.AmbDataNaixementAnteriorA);
        }
        //
        private IQueryable<Alumne> MatchDataInformeAnteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDataInformeAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.DataInformeNESENEE <= request.AmbDataInformeAnteriorA ||
               model.DataInformeNESENoNEE <= request.AmbDataInformeAnteriorA);
        }

        private IQueryable<Alumne> MatchDataInformePosteriorA(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.AmbDataInformeAnteriorA.HasValue)
                return query;

            return query.Where(model =>
               model.DataInformeNESENEE >= request.AmbDataInformeAnteriorA ||
               model.DataInformeNESENoNEE >= request.AmbDataInformeAnteriorA);
        }

        private IQueryable<Alumne> MatchEtapa(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.EtapaId.HasValue)
                return query;

            return query.Where(model =>
                model.EtapaActual != null &&
                model.EtapaActual.Id == request.EtapaId);
        }

        private IQueryable<Alumne> MatchTipusActuacio(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.TipusActuacioId.HasValue)
                return query;

            return query.Where(model =>
                model.Actuacions.Any(a => a.TipusActuacio != null && a.TipusActuacio.Id == request.TipusActuacioId));
        }

        private IQueryable<Alumne> MatchNomCognomsCentre(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!string.IsNullOrWhiteSpace(request.NomCognomsCentre))
                return query;

            var tokens = request.NomCognomsCentre.Split().Select(x => x.Trim()).ToList();

            tokens.ForEach(token =>
                query.Where(model =>
                    ( model.CentreActual != null && model.CentreActual.Nom.Contains(token) ) ||
                    model.Nom.Contains(token) ||
                    model.Cognoms.Contains(token)
                )
            );
            return query;
        }

        private IQueryable<Alumne> MatchTags(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!string.IsNullOrWhiteSpace(request.Tags))
                return query;

            var tokens = request.Tags.Split().Select(x => x.Trim()).ToList();

            tokens.ForEach(token =>
                query.Where(model => model.Tags.Contains(token))
            );
            return query;
        }

        private IQueryable<Alumne> MatchEsActiu(IQueryable<Alumne> query, AlumneSearchParms request)
        {
            if (!request.EsActiu.HasValue)
                return query;

            return query.Where(model =>
                model.EsActiu == request.EsActiu);
        }

        //
        protected override Expression<Func<models.Alumne, dtoo.Alumne>> ToDto
            =>
            project
            .Alumne
            .ToDto;

    }
}

using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using System.Linq;
using Project = DTO.Projections;
using Models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class TipusActuacioSet :
        BLSet<Models.TipusActuacio, Parms.EsActiuParms, Dtoo.TipusActuacio>,
        ITipusActuacioSet
    {
        public TipusActuacioSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<Models.TipusActuacio> GetModels(Parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c => c.Nom);

        protected override Expression<Func<Models.TipusActuacio, Dtoo.TipusActuacio>> ToDto
            =>
            Project
            .TipusActuacio
            .ToDto;
    }
}

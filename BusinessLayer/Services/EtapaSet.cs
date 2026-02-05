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
    public class EtapaSet :
        BLSet<Models.Etapa, Parms.EsActiuParms, Dtoo.Etapa>,
        IEtapaSet
    {
        public EtapaSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<Models.Etapa> GetModels(Parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c => c.Nom);

        protected override Expression<Func<Models.Etapa, Dtoo.Etapa>> ToDto
            =>
            Project
            .Etapa
            .ToDto;
    }
}

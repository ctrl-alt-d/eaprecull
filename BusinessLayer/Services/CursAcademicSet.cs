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
using System.Threading.Tasks;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;

namespace BusinessLayer.Services
{
    public class CursAcademicSet :
        BLSet<Models.CursAcademic, Parms.EsActiuParms, Dtoo.CursAcademic>,
        ICursAcademicSet
    {
        public CursAcademicSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<Models.CursAcademic> GetModels(Parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderByDescending(c => c.AnyInici);

        public async Task<bool?> ElCursPerDefecteEsCorresponAmbLaDataActual()
        {
            var cursActiu =
                await
                GetModels(new Parms.EsActiuParms(true))
                .FirstOrDefaultAsync();

            if (cursActiu == null) return null;

            return cursActiu.AnyInici == DateTime.Now.AddMonths(-8).Year;
        }

        public async Task<OperationResult<Dtoo.CursAcademic>> GetCursActiu()
        {
            var cursActiu =
                await
                GetModels(new Parms.EsActiuParms(true))
                .Select(ToDto)
                .FirstOrDefaultAsync();


            if (cursActiu == null)
            {
                var excepti = new List<BrokenRule>() { new BrokenRule("Cel definir el curs actiu") };
                return new OperationResult<Dtoo.CursAcademic>(excepti);
            }

            return new OperationResult<Dtoo.CursAcademic>(cursActiu);
        }

        protected override Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            Project
            .CursAcademic
            .ToDto;
    }
}

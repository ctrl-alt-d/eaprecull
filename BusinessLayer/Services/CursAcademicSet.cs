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
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;

namespace BusinessLayer.Services
{
    public class CursAcademicSet :
        BLSet<models.CursAcademic, parms.EsActiuParms, dtoo.CursAcademic>,
        ICursAcademicSet
    {
        public CursAcademicSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.CursAcademic> GetModels(parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c => c.Nom);

        public async Task<bool?> ElCursPerDefecteEsCorresponAmbLaDataActual()
        {
            var cursActiu =
                await
                GetModels(new parms.EsActiuParms(true))
                .FirstOrDefaultAsync();
            
            if (cursActiu == null ) return null;

            return cursActiu.AnyInici == DateTime.Now.AddMonths(-9).Year;
        }

        public async Task<OperationResult<dtoo.CursAcademic>> GetCursActiu()
        {
            var cursActiu =
                await
                GetModels(new parms.EsActiuParms(true))
                .Select(ToDto)
                .FirstOrDefaultAsync();
            
            
            if (cursActiu == null )
            {
                var excepti = new List<BrokenRule>() { new BrokenRule("Cel definir el curs actiu") } ;
                return new OperationResult<dtoo.CursAcademic>( excepti );
            }

            return new OperationResult<dtoo.CursAcademic>(cursActiu);            
        }

        protected override Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto 
            =>
            project
            .CursAcademic
            .ToDto;
    }
}

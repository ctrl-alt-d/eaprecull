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

namespace BusinessLayer.Services
{
    public class CursAcademicGetSet :
        BLGetSet<models.CursAcademic, parms.EmptyParms, dtoo.CursAcademic>,
        ICursAcademicGetSet
    {
        public CursAcademicGetSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.CursAcademic> GetModels(parms.EmptyParms request)
            =>
            GetAllModels()
            .OrderBy(c => c.Nom);

        protected override Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto 
            =>
            project
            .CursAcademic
            .ToDto;
    }
}

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
    public class AlumneGetSet :
        BLGetSet<models.Alumne, parms.EmptyParms, dtoo.Alumne>,
        IAlumneGetSet
    {
        public AlumneGetSet(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Alumne> GetModels(parms.EmptyParms request)
            =>
            GetAllModels()
            .Include(x=>x.CentreActual)
            .OrderBy(c=>c.Nom);

        protected override Expression<Func<models.Alumne, dtoo.Alumne>> ToDto
            => 
            (x) =>
            project
            .Alumne
            .ToDto(x);

    }
}

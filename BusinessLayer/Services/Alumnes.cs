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

namespace BusinessLayer.Services
{
    public class Alumnes :
        BLGetItems<models.Alumne, parms.EmptyParms, dtoo.Alumne>,
        IAlumnes
    {
        public Alumnes(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Alumne> GetModels(parms.EmptyParms request)
            =>
            GetAllModels()
            .Include(x=>x.CentreActual)
            .OrderBy(c=>c.Nom);

        protected override dtoo.Alumne ToDto(models.Alumne model)
            =>
            project
            .Alumne
            .ToDto(model);

    }
}

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
using DTO.i.DTOs;
using System;

namespace BusinessLayer.Services
{
    public class ActuacionsByAlumne :
        BLGetSet<models.Actuacio, parms.GetActuacioByAlumneParms, dtoo.Actuacio>,
        IActuacioSetByAlumne
    {
        public ActuacionsByAlumne(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Actuacio> GetModels(GetActuacioByAlumneParms request)
            => 
            GetAllModels()
            .Where(x => x.Alumne.Id == request.IdAlumne);

        protected override Func<models.Actuacio, dtoo.Actuacio> ToDto
            =>
            project.Actuacio.ToDto;

    }
}

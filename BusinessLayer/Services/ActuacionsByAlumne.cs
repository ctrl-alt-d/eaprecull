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

namespace BusinessLayer.Services
{
    public class ActuacionsByAlumne :
        BLGetItems<models.Actuacio, parms.GetActuacioByAlumneParms, dtoo.Actuacio>,
        IActuacionsByAlumne
    {
        public ActuacionsByAlumne(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override IQueryable<models.Actuacio> GetModels(GetActuacioByAlumneParms request)
            => 
            GetAllModels()
            .Where(x => x.Alumne.Id == request.IdAlumne);

        protected override dtoo.Actuacio ToDto(models.Actuacio model)
            =>
            project.Actuacio.ToDto(model);

    }
}

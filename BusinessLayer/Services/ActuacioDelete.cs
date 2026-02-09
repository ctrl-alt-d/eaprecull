using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class ActuacioDelete :
        BLDelete<Models.Actuacio, Dtoo.Actuacio>,
        IActuacioDelete
    {
        public ActuacioDelete(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.Actuacio, Dtoo.Actuacio>> ToDto
            =>
            Project
            .Actuacio
            .ToDto;

        protected override Task PreDelete(Models.Actuacio model)
            =>
            Task.CompletedTask;

        protected override async Task PostDelete(Models.Actuacio model)
        {
            // Carregar l'alumne relacionat
            await LoadReference(model, m => m.Alumne);

            // Decrementar comptador
            model.Alumne.NombreTotalDactuacions--;

            // Actualitzar timestamp
            model.Alumne.DataDarreraModificacio = DateTime.Now;
        }
    }
}

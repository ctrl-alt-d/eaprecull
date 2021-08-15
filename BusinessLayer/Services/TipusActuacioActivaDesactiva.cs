using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using DataModels.Models;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class TipusActuacioActivaDesactiva :
        BLActivaDesactiva<models.TipusActuacio, dtoo.TipusActuacio>,
        ITipusActuacioActivaDesactiva
    {
        public TipusActuacioActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.TipusActuacio, dtoo.TipusActuacio>> ToDto
            =>
            project
            .TipusActuacio
            .ToDto;
        protected override Task Post(models.TipusActuacio model)
            =>
            Task.CompletedTask;

        protected override Task Pre(models.TipusActuacio model)
            =>
            Task.CompletedTask;



    }
}

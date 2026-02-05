using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
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
        BLActivaDesactiva<Models.TipusActuacio, Dtoo.TipusActuacio>,
        ITipusActuacioActivaDesactiva
    {
        public TipusActuacioActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.TipusActuacio, Dtoo.TipusActuacio>> ToDto
            =>
            Project
            .TipusActuacio
            .ToDto;
        protected override Task Post(Models.TipusActuacio model)
            =>
            Task.CompletedTask;

        protected override Task Pre(Models.TipusActuacio model)
            =>
            Task.CompletedTask;



    }
}

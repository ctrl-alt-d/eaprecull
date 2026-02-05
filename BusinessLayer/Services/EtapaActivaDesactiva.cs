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
    public class EtapaActivaDesactiva :
        BLActivaDesactiva<Models.Etapa, Dtoo.Etapa>,
        IEtapaActivaDesactiva
    {
        public EtapaActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.Etapa, Dtoo.Etapa>> ToDto
            =>
            Project
            .Etapa
            .ToDto;

        protected override Task Post(Models.Etapa model)
            =>
            Task.CompletedTask;

        protected override Task Pre(Models.Etapa model)
            =>
            Task.CompletedTask;


    }
}

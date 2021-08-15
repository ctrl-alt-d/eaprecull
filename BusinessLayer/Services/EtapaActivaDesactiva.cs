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
    public class EtapaActivaDesactiva :
        BLActivaDesactiva<models.Etapa, dtoo.Etapa>,
        IEtapaActivaDesactiva
    {
        public EtapaActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.Etapa, dtoo.Etapa>> ToDto
            =>
            project
            .Etapa
            .ToDto;

        protected override Task Post(models.Etapa model)
            =>
            Task.CompletedTask;

        protected override Task Pre(models.Etapa model)
            =>
            Task.CompletedTask;


    }
}

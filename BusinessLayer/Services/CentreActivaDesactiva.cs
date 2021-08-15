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
    public class CentreActivaDesactiva :
        BLActivaDesactiva<models.Centre, dtoo.Centre>,
        ICentreActivaDesactiva
    {
        public CentreActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.Centre, dtoo.Centre>> ToDto
            =>
            project
            .Centre
            .ToDto;
        protected override Task Post(models.Centre model)
            =>
            Task.CompletedTask;

        protected override Task Pre(models.Centre model)
            =>
            Task.CompletedTask;



    }
}

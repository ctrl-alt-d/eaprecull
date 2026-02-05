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
    public class CentreActivaDesactiva :
        BLActivaDesactiva<Models.Centre, Dtoo.Centre>,
        ICentreActivaDesactiva
    {
        public CentreActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.Centre, Dtoo.Centre>> ToDto
            =>
            Project
            .Centre
            .ToDto;
        protected override Task Post(Models.Centre model)
            =>
            Task.CompletedTask;

        protected override Task Pre(Models.Centre model)
            =>
            Task.CompletedTask;



    }
}

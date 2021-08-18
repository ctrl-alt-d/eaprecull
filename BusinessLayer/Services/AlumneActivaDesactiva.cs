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
    public class AlumneActivaDesactiva :
        BLActivaDesactiva<models.Alumne, dtoo.Alumne>,
        IAlumneActivaDesactiva
    {
        public AlumneActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.Alumne, dtoo.Alumne>> ToDto
            =>
            project
            .Alumne
            .ToDto;
        protected override Task Post(models.Alumne model)
            =>
            Task.CompletedTask;

        protected override Task Pre(models.Alumne model)
            =>
            Task.CompletedTask;



    }
}

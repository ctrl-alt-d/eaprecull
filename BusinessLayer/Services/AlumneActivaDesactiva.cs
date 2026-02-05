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
    public class AlumneActivaDesactiva :
        BLActivaDesactiva<Models.Alumne, Dtoo.Alumne>,
        IAlumneActivaDesactiva
    {
        public AlumneActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.Alumne, Dtoo.Alumne>> ToDto
            =>
            Project
            .Alumne
            .ToDto;
        protected override Task Post(Models.Alumne model)
            =>
            Task.CompletedTask;

        protected override Task Pre(Models.Alumne model)
            =>
            Task.CompletedTask;



    }
}

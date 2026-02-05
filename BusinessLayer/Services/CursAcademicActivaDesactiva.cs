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
using System.Linq;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class CursAcademicActivaDesactiva :
        BLActivaDesactiva<Models.CursAcademic, Dtoo.CursAcademic>,
        ICursAcademicActivaDesactiva
    {
        public CursAcademicActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            Project
            .CursAcademic
            .ToDto;

        protected override Task Post(Models.CursAcademic model)
            =>
            DeixaNomesUnActiu(model);

        protected virtual async Task DeixaNomesUnActiu(Models.CursAcademic model)
        {
            if (!model.EsActiu)
                return;

            // Si el nostre element ha passat a actiu, la resta desactivar-los
            var tots =
                await
                GetContext()
                .CursosAcademics
                .Where(x => x != model)
                .ToListAsync();

            tots
                .ForEach(x => x.EsActiu = false);

        }

        protected override Task Pre(Models.CursAcademic model)
            =>
            Task.CompletedTask;

    }
}

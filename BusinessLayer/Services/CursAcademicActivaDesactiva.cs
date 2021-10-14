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
using System.Linq;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class CursAcademicActivaDesactiva :
        BLActivaDesactiva<models.CursAcademic, dtoo.CursAcademic>,
        ICursAcademicActivaDesactiva
    {
        public CursAcademicActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto
            =>
            project
            .CursAcademic
            .ToDto;

        protected override Task Post(models.CursAcademic model)
            =>
            DeixaNomesUnActiu(model);

        protected virtual async Task DeixaNomesUnActiu(models.CursAcademic model)
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

        protected override Task Pre(models.CursAcademic model)
            =>
            Task.CompletedTask;

    }
}

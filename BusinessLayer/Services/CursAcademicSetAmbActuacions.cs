using BusinessLayer.Abstract.Services;
using dtoo = DTO.o.DTOs;
using System.Linq;
using project = DTO.Projections;
using models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class CursAcademicSetAmbActuacions :
        CursAcademicSet,
        ICursAcademicSetAmbActuacions
    {
        public CursAcademicSetAmbActuacions(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto
            =>
            project
            .CursAcademicAmbActuacions
            .ToDto;
    }
}

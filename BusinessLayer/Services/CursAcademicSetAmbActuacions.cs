using BusinessLayer.Abstract.Services;
using Dtoo = DTO.o.DTOs;
using System.Linq;
using Project = DTO.Projections;
using Models = DataModels.Models;
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

        protected override Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            Project
            .CursAcademicAmbActuacions
            .ToDto;
    }
}

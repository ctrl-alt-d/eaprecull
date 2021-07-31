using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;

namespace BusinessLayer.Services
{
    public class CursAcademicCreate : 
        BLCreate<models.CursAcademic, parms.CursAcademicCreateParms, dtoo.CursAcademic>,
        ICursAcademicCreate
    {
        public CursAcademicCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        protected override models.CursAcademic SetValues(CursAcademicCreateParms parm)
            =>
            new()
            {
                AnyInici = parm.AnyInici,
                EsElCursActual = parm.EsElCursActual,
                Nom = $"{parm.AnyInici}-{parm.AnyInici+1}"
            };
            
        protected override dtoo.CursAcademic ToDto(models.CursAcademic parm)
            =>
            project
            .CursAcademic
            .ToDto(parm);
    }
}

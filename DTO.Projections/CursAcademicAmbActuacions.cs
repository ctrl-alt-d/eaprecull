using System;
using System.Linq;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class CursAcademicAmbActuacions
    {
        public static Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            model
            =>
            new Dtoo.CursAcademicAmbActuacions(
                model.Id,
                model.AnyInici,
                model.Nom,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio,
                model.Actuacions.Count()
            );
    }
}

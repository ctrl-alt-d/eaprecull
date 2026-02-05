using System;
using System.Linq;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class CursAcademicAmbActuacions
    {
        public static Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto
            =>
            model
            =>
            new dtoo.CursAcademicAmbActuacions(
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

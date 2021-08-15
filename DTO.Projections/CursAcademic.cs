using System;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class CursAcademic
    {
        public static Expression<Func<models.CursAcademic, dtoo.CursAcademic>> ToDto 
            =>
            model
            =>
            new (
                model.Id,
                model.AnyInici,
                model.Nom,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio
            );
    }
}

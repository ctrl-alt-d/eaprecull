using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class CursAcademic
    {
        public static Expression<Func<Models.CursAcademic, Dtoo.CursAcademic>> ToDto
            =>
            model
            =>
            new(
                model.Id,
                model.AnyInici,
                model.Nom,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio
            );
    }
}

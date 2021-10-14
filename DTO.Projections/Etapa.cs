using System;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Etapa
    {
        public static Expression<Func<models.Etapa, dtoo.Etapa>> ToDto
            =>
            model
            =>
            new(
                model.Id,
                model.Codi,
                model.Nom,
                model.SonEstudisObligatoris,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio
            );
    }
}

using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class Etapa
    {
        public static Expression<Func<Models.Etapa, Dtoo.Etapa>> ToDto
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

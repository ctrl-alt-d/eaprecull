using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class TipusActuacio
    {
        public static Expression<Func<Models.TipusActuacio, Dtoo.TipusActuacio>> ToDto
            =>
            model
            =>
            new(
              model.Id,
              model.Codi,
              model.Nom,
              model.EsActiu,
              model.Etiqueta,
              model.Descripcio
            );
    }
}

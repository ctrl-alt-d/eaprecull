using System;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Centre
    {
        public static Expression<Func<models.Centre, dtoo.Centre>> ToDto 
            =>
            model
            =>
            new (
                model.Id, model.Codi, model.Nom, model.EsActiu, model.Etiqueta, model.Descripcio
            );
    }
}

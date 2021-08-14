using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Etapa
    {
        public static dto.Etapa ToDto(this models.Etapa model)
            =>
            new dto.Etapa(
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

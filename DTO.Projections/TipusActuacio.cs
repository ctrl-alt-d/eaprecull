using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class TipusActuacio
    {
        public static dto.TipusActuacio ToDto(this models.TipusActuacio model)
            =>
            new dto.TipusActuacio(
              model.Id,
              model.Codi,
              model.Nom,
              model.EsActiu,
              model.Etiqueta,
              model.Descripcio
            );
    }
}

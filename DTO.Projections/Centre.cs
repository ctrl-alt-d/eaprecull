using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Centre
    {
        public static dto.Centre ToDto(this models.Centre model)
            =>
            new (
                model.Id, model.Codi, model.Nom, model.EsActiu, model.Etiqueta, model.Descripcio
            );
    }
}

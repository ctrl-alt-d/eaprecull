using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class CursAcademic
    {
        public static dto.CursAcademic ToDto(this models.CursAcademic model)
            =>
            new dto.CursAcademic(
                model.Id,
                model.AnyInici,
                model.Nom,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio
            );
    }
}

using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Alumne
    {
        public static dto.Alumne ToDto(this models.Alumne model)
            =>
            new dto.Alumne(
                model.Id,
                model.Non,
                model.Cognoms,
                model.DataNaixement,
                model.CentreActual,
                model.CursDarreraActualitacioDades,
                model.EtapaActual,
                model.DataInformeNESENEE,
                model.ObservacionsNESENEE,
                model.DataInformeNESENoNEE,
                model.ObservacionsNESENoNEE,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio                
            );
    }
}

using System;
using dto = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Actuacio
    {
        public static dto.Actuacio ToDto(this models.Actuacio model)
            =>
            new dto.Actuacio(
                model.Id,
                model.Alumne,
                model.TipusActuacio,
                model.ObservacionsTipusActuacio,
                model.MomentDeLactuacio,
                model.CursActuacio,
                model.CentreAlMomentDeLactuacio,
                model.EtapaAlMomentDeLactuacio,
                model.NivellAlMomentDeLactuacio,
                model.MinutsDuradaActuacio,
                model.DescripcioActuacio,
                model.Etiqueta,
                model.Descripcio                
            );
    }
}

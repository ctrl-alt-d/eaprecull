using System;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Actuacio
    {
        public static Expression<Func<models.Actuacio, dtoo.Actuacio>> ToDto
            =>
            model
            =>
            new(
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

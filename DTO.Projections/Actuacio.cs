using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class Actuacio
    {
        public static Expression<Func<Models.Actuacio, Dtoo.Actuacio>> ToDto
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

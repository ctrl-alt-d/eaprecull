using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class Alumne
    {
        public static Expression<Func<Models.Alumne, Dtoo.Alumne>> ToDto
            =>
            model
            =>
            new(
                model.Id,
                model.Nom,
                model.Cognoms,
                model.DataNaixement,
                model.CentreActual,
                model.CursDarreraActualitacioDades,
                model.EtapaActual,
                model.NivellActual,
                model.DataInformeNESENEE,
                model.ObservacionsNESENEE,
                model.DataInformeNESENoNEE,
                model.ObservacionsNESENoNEE,
                model.EsActiu,
                model.Etiqueta,
                model.Descripcio,
                model.Tags,
                model.NombreTotalDactuacions
            );
    }
}

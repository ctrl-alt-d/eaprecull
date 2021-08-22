using System;
using System.Linq.Expressions;
using dtoo = DTO.o.DTOs;
using models = DataModels.Models;

namespace DTO.Projections
{
    public static class Alumne
    {
        public static Expression<Func<models.Alumne, dtoo.Alumne>> ToDto 
            =>
            model
            =>
            new (
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

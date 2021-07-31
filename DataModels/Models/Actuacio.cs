using System;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{

    public class Actuacio : IIdEtiquetaDescripcio, IModel
    {
        public int Id { get; set; }
        public Alumne Alumne { get; set; } = default!;
        public TipusActuacio TipusActuacio { get; set; } = default!;
        public string ObservacionsTipusActuacio { get; set; } = string.Empty;
        public DateTime MomentDeLactuacio { get; set; }
        public CursAcademic CursActuacio { get; set; } = default!;
        public Centre CentreAlMomentDeLactuacio { get; set; } = default!;
        public Etapa EtapaAlMomentDeLactuacio { get; set; } = default!;
        public string NivellAlMomentDeLactuacio { get; set; } = string.Empty;
        public int MinutsDuradaActuacio { get; set; }
        public string DescripcioActuacio { get; set; } = string.Empty;

        // IEtiquetaDescripcio
        public string Etiqueta => $"{MomentDeLactuacio} {Alumne.Etiqueta}";
        public string Descripcio => $"{TipusActuacio.Etiqueta}";
    }
}

using System;
using System.Collections.Generic;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{    
    public class Alumne : IIdEtiquetaDescripcio, IActiu, IModel
    {
        public int Id { get; set; }
        public string Non { get; set; } = string.Empty;
        public string Cognoms { get; set; } = string.Empty;
        public DateTime? DataNaixement { get; set; }
        public Centre? CentreActual { get; set; }
        public CursAcademic CursDarreraActualitacioDades { get; set; } = default!;
        public Etapa? EtapaActual { get; set; }
        public DateTime? DataInformeNESENEE { get; set; }
        public string ObservacionsNESENEE { get; set; } = string.Empty;
        public DateTime? DataInformeNESENoNEE { get; set; }
        public string ObservacionsNESENoNEE { get; set; } = string.Empty;

        // IActiu
        public bool EsActiu { get; set; }

        // IEtiquetaDescripcio
        public string Etiqueta => $"{Cognoms}, {Non}";
        public string Descripcio =>
            $"{DataNaixement} {CentreActual?.Etiqueta ?? ""} " +
            $"{EtapaActual?.Etiqueta ?? ""} {_NESEEE_txt}{_NESENoEE_txt}";
        public string _NESEEE_txt => DataInformeNESENEE != null ? " [NESE NEE]" : "";
        public string _NESENoEE_txt => DataInformeNESENoNEE != null ? " [NESE No NEE]" : "";

        //
        public List<Actuacio> Actuacions { get; set; } = new();
    }
}

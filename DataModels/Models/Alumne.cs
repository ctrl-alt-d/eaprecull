using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace DataModels.Models
{
    public class Alumne : IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Cognoms { get; set; } = string.Empty;
        public DateTime? DataNaixement { get; set; }
        public Centre? CentreActual { get; set; }
        public CursAcademic CursDarreraActualitacioDades { get; set; } = default!;
        public Etapa? EtapaActual { get; set; }
        public string NivellActual { get; set; } = string.Empty;
        public DateTime? DataInformeNESENEE { get; set; }
        public string ObservacionsNESENEE { get; set; } = string.Empty;
        public DateTime? DataInformeNESENoNEE { get; set; }
        public string ObservacionsNESENoNEE { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;

        // IActiu
        public bool EsActiu { get; set; }
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => $"{Cognoms}, {Nom}";
        public string Descripcio =>
            $"{DataNaixement?.ToString("d.M.yyyy")} {CentreActual?.Etiqueta ?? ""} " +
            $"{EtapaActual?.Etiqueta ?? ""} {_NESEEE_txt}{_NESENoEE_txt}";
        public string _NESEEE_txt => DataInformeNESENEE != null ? " [NESE NEE]" : "";
        public string _NESENoEE_txt => DataInformeNESENoNEE != null ? " [NESE No NEE]" : "";

        // Caches
        public int NombreTotalDactuacions { get; set; }
        public DateTime? DataDarreraActuacio { get; set; }
        public DateTime DataDarreraModificacio { get; set; } = DateTime.Now;

        //
        public List<Actuacio> Actuacions { get; set; } = new();

    }
}

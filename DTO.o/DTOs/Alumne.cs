using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{    
    public class Alumne : IEtiquetaDescripcio, IActiu, IDTOo
    {
        public Alumne(
            int id,
            string non,
            string cognoms,
            DateTime? dataNaixement,
            IIdEtiquetaDescripcio? centreActual,
            IIdEtiquetaDescripcio cursDarreraActualitacioDades,
            IIdEtiquetaDescripcio? etapaActual,
            DateTime? dataInformeNESENEE,
            string observacionsNESENEE,
            DateTime? dataInformeNESENoNEE,
            string observacionsNESENoNEE,
            bool esActiu,
            string etiqueta,
            string descripcio)
        {
            Id = id;
            Non = non;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActual = centreActual;
            CursDarreraActualitacioDades = cursDarreraActualitacioDades;
            EtapaActual = etapaActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id { get; set; }
        public string Non { get; }= string.Empty;
        public string Cognoms { get; }= string.Empty;
        public DateTime? DataNaixement { get; set; }
        public IIdEtiquetaDescripcio? CentreActual { get; set; }
        public IIdEtiquetaDescripcio CursDarreraActualitacioDades { get; }= default!;
        public IIdEtiquetaDescripcio? EtapaActual { get; set; }
        public DateTime? DataInformeNESENEE { get; set; }
        public string ObservacionsNESENEE { get; }= string.Empty;
        public DateTime? DataInformeNESENoNEE { get; set; }
        public string ObservacionsNESENoNEE { get; }= string.Empty;
        public bool EsActiu { get; set; }
        public string Etiqueta{ get; }= string.Empty;
        public string Descripcio { get; }= string.Empty;
    }
}

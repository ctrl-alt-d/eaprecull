using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{    
    public class Alumne : IIdEtiquetaDescripcio, IActiu, IDTOo
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
            string descripcio,
            string tags)
        {
            Id = id;
            Nom = non;
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
            Tags = tags;
        }

        public int Id { get; set; }
        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime? DataNaixement { get; set; }
        public IIdEtiquetaDescripcio? CentreActual { get; set; }
        public IIdEtiquetaDescripcio CursDarreraActualitacioDades { get; }
        public IIdEtiquetaDescripcio? EtapaActual { get; set; }
        public DateTime? DataInformeNESENEE { get; set; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; set; }
        public string ObservacionsNESENoNEE { get; }
        public bool EsActiu { get; set; }
        public string Etiqueta{ get; }
        public string Descripcio { get; }
        public string Tags { get; }
    }
}

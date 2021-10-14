using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class Alumne : IIdEtiquetaDescripcio, IActiu, IDTOo
    {
        public Alumne(int id, string nom, string cognoms, DateTime? dataNaixement, IIdEtiquetaDescripcio? centreActual, IIdEtiquetaDescripcio cursDarreraActualitacioDades, IIdEtiquetaDescripcio? etapaActual, string nivellActual, DateTime? dataInformeNESENEE, string observacionsNESENEE, DateTime? dataInformeNESENoNEE, string observacionsNESENoNEE, bool esActiu, string etiqueta, string descripcio, string tags, int nombreActuacions)
        {
            Id = id;
            Nom = nom;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActual = centreActual;
            CursDarreraActualitacioDades = cursDarreraActualitacioDades;
            EtapaActual = etapaActual;
            NivellActual = nivellActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
            Tags = tags;
            NombreActuacions = nombreActuacions;
        }

        public int Id { get; set; }
        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime? DataNaixement { get; }
        public IIdEtiquetaDescripcio? CentreActual { get; }
        public IIdEtiquetaDescripcio CursDarreraActualitacioDades { get; }
        public IIdEtiquetaDescripcio? EtapaActual { get; }
        public string NivellActual { get; }
        public DateTime? DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public bool EsActiu { get; }
        public string Etiqueta { get; }
        public string Descripcio { get; }
        public string Tags { get; }
        public int NombreActuacions { get; }
    }
}

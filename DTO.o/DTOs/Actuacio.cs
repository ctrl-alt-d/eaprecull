using System;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{

    public class Actuacio : IIdEtiquetaDescripcio, IDTOo
    {
        public Actuacio(
            int id,
            IIdEtiquetaDescripcio alumne,
            IIdEtiquetaDescripcio tipusActuacio,
            string observacionsTipusActuacio,
            DateTime momentDeLactuacio,
            IIdEtiquetaDescripcio cursActuacio,
            IIdEtiquetaDescripcio centreAlMomentDeLactuacio,
            IIdEtiquetaDescripcio etapaAlMomentDeLactuacio,
            string nivellAlMomentDeLactuacio,
            int minutsDuradaActuacio,
            string descripcioActuacio,
            string etiqueta,
            string descripcio)
        {
            Id = id;
            Alumne = alumne;
            TipusActuacio = tipusActuacio;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            CentreAlMomentDeLactuacio = centreAlMomentDeLactuacio;
            EtapaAlMomentDeLactuacio = etapaAlMomentDeLactuacio;
            NivellAlMomentDeLactuacio = nivellAlMomentDeLactuacio;
            MinutsDuradaActuacio = minutsDuradaActuacio;
            DescripcioActuacio = descripcioActuacio;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id { get; }
        public IIdEtiquetaDescripcio Alumne { get; }
        public IIdEtiquetaDescripcio TipusActuacio { get; }
        public string ObservacionsTipusActuacio { get; }
        public DateTime MomentDeLactuacio { get; }
        public IIdEtiquetaDescripcio CursActuacio { get; }
        public IIdEtiquetaDescripcio CentreAlMomentDeLactuacio { get; }
        public IIdEtiquetaDescripcio EtapaAlMomentDeLactuacio { get; }
        public string NivellAlMomentDeLactuacio { get; }
        public int MinutsDuradaActuacio { get; }
        public string DescripcioActuacio { get; }

        // IEtiquetaDescripcio
        public string Etiqueta { get; }
        public string Descripcio { get; }
    }
}

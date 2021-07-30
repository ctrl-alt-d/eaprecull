using System;
using CommonInterfaces;

namespace DTO.o.DTOs
{

    public class Actuacio : IEtiquetaDescripcio
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
        public IIdEtiquetaDescripcio Alumne { get;  } = default!;
        public IIdEtiquetaDescripcio TipusActuacio { get;  } = default!;
        public string ObservacionsTipusActuacio { get;  } = string.Empty;
        public DateTime MomentDeLactuacio { get;  }
        public IIdEtiquetaDescripcio CursActuacio { get;  } = default!;
        public IIdEtiquetaDescripcio CentreAlMomentDeLactuacio { get;  } = default!;
        public IIdEtiquetaDescripcio EtapaAlMomentDeLactuacio { get;  } = default!;
        public string NivellAlMomentDeLactuacio { get;  } = string.Empty;
        public int MinutsDuradaActuacio { get;  }
        public string DescripcioActuacio { get;  } = string.Empty;

        // IEtiquetaDescripcio
        public string Etiqueta {get; }
        public string Descripcio {get;}
    }
}

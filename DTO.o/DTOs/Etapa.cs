using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace DTO.o.DTOs
{
    public class Etapa: IEtiquetaDescripcio, IActiu
    {
        public Etapa(int id, string codi, string nom, bool esActiu, string etiqueta, string descripcio)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id {get;  }
        public string Codi {get;  } = string.Empty; // Ex: BAT, ESO
        public string Nom {get;  }  = string.Empty; // Ex: Batxillerat

        // IActiu
        public bool EsActiu {get; }

        // IEtiquetaDescripcio
        public string Etiqueta {get;  } = string.Empty; 
        public string Descripcio {get;  } = string.Empty; 
    }
}

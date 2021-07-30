using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace DTO.o.DTOs
{
    public class CursAcademic: IEtiquetaDescripcio
    {
        public CursAcademic(int id, int anyInici, string nom, bool esElCursActual, string etiqueta, string descripcio)
        {
            Id = id;
            AnyInici = anyInici;
            Nom = nom;
            EsElCursActual = esElCursActual;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id {get;  }
        public int AnyInici {get; }
        public string Nom {get; } = string.Empty;
        public bool EsElCursActual {get; }

        // IEtiquetaDescripcio
        public string Etiqueta {get; } = string.Empty;
        public string Descripcio {get; } = string.Empty;
    }
}

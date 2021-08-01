using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class Centre: IEtiquetaDescripcio, IActiu, IDTOo
    {
        public Centre(int id, string codi, string nom, bool esActiu, string etiqueta, string descripcio)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id {get; }
        public string Codi {get; } = string.Empty;
        public string Nom {get; } = string.Empty;

        // IActiu
        public bool EsActiu {get; }

        public string Etiqueta {get; } = string.Empty;
        public string Descripcio {get; } = string.Empty;

    }
}

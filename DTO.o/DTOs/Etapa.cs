using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class Etapa : IIdEtiquetaDescripcio, IActiu, IDTOo
    {
        public Etapa(int id, string codi, string nom, bool sonEstudisObligatoris, bool esActiu, string etiqueta, string descripcio)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            SonEstudisObligatoris = sonEstudisObligatoris;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id { get; }
        public string Codi { get; } = string.Empty; // Ex: BAT, ESO
        public string Nom { get; } = string.Empty; // Ex: Batxillerat

        public bool SonEstudisObligatoris { get; }

        // IActiu
        public bool EsActiu { get; }

        // IEtiquetaDescripcio
        public string Etiqueta { get; } = string.Empty;
        public string Descripcio { get; } = string.Empty;
    }
}

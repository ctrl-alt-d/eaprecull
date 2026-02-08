using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class CursAcademic : IIdEtiquetaDescripcio, IDTOo, IActiu
    {
        public CursAcademic(int id, int anyInici, string nom, bool esActiu, string etiqueta, string descripcio, int nombreActuacions)
        {
            Id = id;
            AnyInici = anyInici;
            Nom = nom;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
            NombreActuacions = nombreActuacions;
        }

        public int Id { get; }
        public int AnyInici { get; }
        public string Nom { get; } = string.Empty;
        public bool EsActiu { get; }
        public int NombreActuacions { get; }

        // IEtiquetaDescripcio
        public string Etiqueta { get; } = string.Empty;
        public string Descripcio { get; } = string.Empty;

        public override string ToString() => Etiqueta;
    }
}

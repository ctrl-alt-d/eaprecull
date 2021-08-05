using System;

namespace DTO.i.DTOs
{
    public class CentreUpdateParms : IDtoi
    {
        public CentreUpdateParms(int id, string codi, string nom, bool esActiu)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
        }

        public int Id {get;}
        public string Codi { get; }
        public string Nom { get; }

        // IActiu
        public bool EsActiu { get; }
    }
}
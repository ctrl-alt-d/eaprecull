using System;

namespace DTO.i.DTOs
{
    public class CentreCreateParms : IDtoi
    {
        public CentreCreateParms(string codi, string nom, bool esActiu)
        {
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
        }

        public string Codi { get; }
        public string Nom { get; }

        // IActiu
        public bool EsActiu { get; }
    }
}

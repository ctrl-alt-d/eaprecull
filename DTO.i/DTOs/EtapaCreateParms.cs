using System;

namespace DTO.i.DTOs
{
    public class EtapaCreateParms : IDtoi
    {
        public EtapaCreateParms(
            string codi,
            string nom,
            bool sonEstudisObligatoris,
            bool esActiu)
        {
            Codi = codi;
            Nom = nom;
            SonEstudisObligatoris = sonEstudisObligatoris;
            EsActiu = esActiu;
        }

        public string Codi { get; }
        public string Nom { get; }
        public bool SonEstudisObligatoris {get;} 
        // IActiu
        public bool EsActiu { get; }
    }
}

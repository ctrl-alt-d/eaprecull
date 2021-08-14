using System;
using CommonInterfaces;

namespace DTO.i.DTOs
{
    public class EtapaUpdateParms : IDtoi, IId
    {
        public EtapaUpdateParms(int id, string codi, string nom, bool sonEstudisObligatoris, bool esActiu)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            SonEstudisObligatoris = sonEstudisObligatoris;
            EsActiu = esActiu;
        }

        public int Id {get;}
        public string Codi { get; }
        public string Nom { get; }
        public bool SonEstudisObligatoris {get;} 
        // IActiu
        public bool EsActiu { get; }
    }
}

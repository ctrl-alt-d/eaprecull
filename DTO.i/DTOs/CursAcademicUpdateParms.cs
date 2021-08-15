using System;
using CommonInterfaces;

namespace DTO.i.DTOs
{
    public class CursAcademicUpdateParms : IDtoi, IId
    {
        public CursAcademicUpdateParms(int id, int anyInici, bool esActiu)
        {
            Id = id;
            AnyInici = anyInici;
            EsActiu = esActiu;
        }

        public int Id {get;}
        public int AnyInici {get; }
        public bool EsActiu {get; }

    }
}

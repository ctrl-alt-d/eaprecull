using System;

namespace DTO.i.DTOs
{
    public class CursAcademicCreateParms : IDtoi
    {
        public CursAcademicCreateParms(int anyInici, bool esActiu)
        {
            AnyInici = anyInici;
            EsActiu = esActiu;
        }

        public int AnyInici {get; }
        public bool EsActiu {get; }

    }
}

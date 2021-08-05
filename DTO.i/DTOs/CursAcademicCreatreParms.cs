using System;

namespace DTO.i.DTOs
{
    public class CursAcademicCreateParms : IDtoi
    {
        public CursAcademicCreateParms(int anyInici, bool EsActiu)
        {
            AnyInici = anyInici;
            EsActiu = EsActiu;
        }

        public int AnyInici {get; }
        public bool EsActiu {get; }

    }
}

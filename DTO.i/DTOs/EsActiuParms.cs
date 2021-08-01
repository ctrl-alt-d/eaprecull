using System;

namespace DTO.i.DTOs
{
    public class EsActiuParms: IDtoi
    {
        public EsActiuParms(bool? esActiu)
        {
            EsActiu = esActiu;
        }

        public bool? EsActiu {get; }
    }
}

using System;

namespace DTO.i.DTOs
{
    public class ActivaDesactivaParms : IDtoi
    {
        public ActivaDesactivaParms(bool esActiu)
        {
            EsActiu = esActiu;
        }

        public bool EsActiu { get; }
    }
}

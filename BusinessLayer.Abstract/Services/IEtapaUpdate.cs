using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using BusinessLayer.Abstract.Generic;

namespace BusinessLayer.Abstract.Services
{
    public interface IEtapaUpdate : IUpdate<Dtoo.Etapa, Parms.EtapaUpdateParms>
    {
    }
}

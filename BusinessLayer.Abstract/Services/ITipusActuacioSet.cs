using BusinessLayer.Abstract.Generic;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface ITipusActuacioSet : ISet<Parms.EsActiuParms, Dtoo.TipusActuacio>
    {
    }

}
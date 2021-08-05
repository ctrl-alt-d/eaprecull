using BusinessLayer.Abstract.Generic;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface ICentreGetSet: IGetItems<parms.EsActiuParms, dtoo.Centre>
    {        
    }

}
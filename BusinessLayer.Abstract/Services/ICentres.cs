using BusinessLayer.Abstract.Generic;
using dtoi = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface ICentres: IQuery<dtoo.Centre, dtoi.EsActiuParms>
    {        
    }

}
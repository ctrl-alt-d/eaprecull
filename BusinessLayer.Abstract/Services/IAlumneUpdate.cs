using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using BusinessLayer.Abstract.Generic;

namespace BusinessLayer.Abstract.Services
{
    public interface IAlumneUpdate : IUpdate<dtoo.Alumne, parms.AlumneUpdateParms>
    {
    }
}

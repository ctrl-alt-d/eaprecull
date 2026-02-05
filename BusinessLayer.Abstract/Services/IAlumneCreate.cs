using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using BusinessLayer.Abstract.Generic;

namespace BusinessLayer.Abstract.Services
{
    public interface IAlumneCreate : ICreate<Dtoo.Alumne, Parms.AlumneCreateParms>
    {
    }
}

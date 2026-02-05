using BusinessLayer.Abstract.Generic;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface IAlumneSet : ISet<Parms.AlumneSearchParms, Dtoo.Alumne>
    {
    }

}
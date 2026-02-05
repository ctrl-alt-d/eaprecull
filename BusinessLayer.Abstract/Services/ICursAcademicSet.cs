using BusinessLayer.Abstract.Generic;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Services
{
    public interface ICursAcademicSet : ISet<Parms.EsActiuParms, Dtoo.CursAcademic>
    {
        Task<bool?> ElCursPerDefecteEsCorresponAmbLaDataActual();

        Task<OperationResult<Dtoo.CursAcademic>> GetCursActiu();
    }

}
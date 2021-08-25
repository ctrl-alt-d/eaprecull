using BusinessLayer.Abstract.Generic;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Services
{
    public interface ICursAcademicSet: ISet<parms.EsActiuParms, dtoo.CursAcademic>
    {        
        Task<bool?> ElCursPerDefecteEsCorresponAmbLaDataActual();

        Task<OperationResult<dtoo.CursAcademic>> GetCursActiu();
    }

}
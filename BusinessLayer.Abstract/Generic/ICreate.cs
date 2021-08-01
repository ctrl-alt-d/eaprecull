using System.Threading.Tasks;
using CommonInterfaces;
using DTO.i;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface ICreate<TDTO, TParm>
    where TDTO: IDTOo, IEtiquetaDescripcio
    where TParm: IDtoi
    {
        Task<OperationResult<TDTO>> Create(TParm parm);
    }
}
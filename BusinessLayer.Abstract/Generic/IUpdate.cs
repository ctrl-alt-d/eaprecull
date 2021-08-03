using System.Threading.Tasks;
using CommonInterfaces;
using DTO.i;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IUpdate<TDTOo, TParm>
        :IBLOperation
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TParm: IDtoi
    {
        Task<OperationResult<TDTOo>> Update(int id, TParm request);
    }
}
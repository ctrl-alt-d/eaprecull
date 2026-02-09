using System.Threading.Tasks;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IDelete<TDTOo>
        : IBLOperation
            where TDTOo : IDTOo, IEtiquetaDescripcio
    {
        Task<OperationResult<TDTOo>> Delete(IId id);
    }
}
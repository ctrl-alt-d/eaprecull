using CommonInterfaces;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IDelete<TDTOo>
        :IBLOperation
            where TDTOo: IDTOo, IEtiquetaDescripcio
    {
        OperationResult<TDTOo> Delete(int id);
    }
}
using CommonInterfaces;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IDelete<TDTOo> where TDTOo: IDTOo, IEtiquetaDescripcio
    {
        OperationResult<TDTOo> Delete(int id);
    }
}
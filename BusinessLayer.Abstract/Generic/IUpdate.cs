using CommonInterfaces;
using DTO.i;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IUpdate<TParm, TDTOo> 
        where TDTOo: IDTOo, IEtiquetaDescripcio
        where TParm: IDtoi
    {
        OperationResult<TDTOo> Update(TParm request);
    }
}
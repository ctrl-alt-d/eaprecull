using CommonInterfaces;
using DTO.i;

namespace BusinessLayer.Abstract.Generic
{
    public interface IUpdate<T> where T: IEtiquetaDescripcio
    {
        OperationResult<T> Update<P>(P request) where P: IDtoi;
    }
}
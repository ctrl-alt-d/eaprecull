using CommonInterfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IGet<T> where T: IEtiquetaDescripcio
    {
        OperationResult<T> Get(int id);
    }
}
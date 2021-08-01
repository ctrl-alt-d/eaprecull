using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace BusinessLayer.Abstract
{

    public class OperationResult<T> where T: IEtiquetaDescripcio
    {
        public OperationResult(List<BrokenRule> brokenRules)
        {
            BrokenRules = brokenRules;
        }

        public OperationResult(T data)
        {
            Data = data;
        }

        public T? Data {get;}
        public List<BrokenRule> BrokenRules {get; } = new();

    }
}

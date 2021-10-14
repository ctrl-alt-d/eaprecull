using System;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using CommonInterfaces;

namespace BusinessLayer.Abstract
{

    public class OperationResults<T> where T : IEtiquetaDescripcio
    {
        public OperationResults(List<T>? data, int total, int takeRequested)
        {
            Data = data;
            Total = total;
            TakeRequested = takeRequested;
        }

        public OperationResults(List<BrokenRule> brokenRules)
        {
            BrokenRules = brokenRules;
        }

        public List<T>? Data { get; }
        public List<BrokenRule> BrokenRules { get; } = new();
        public int Total { get; }
        public int TakeRequested { get; }

    }
}

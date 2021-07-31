﻿using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace BusinessLayer.Abstract
{

    public class OperationResults<T> where T: IEtiquetaDescripcio
    {
        public OperationResults(List<T>? data)
        {
            Data = data;
        }

        public OperationResults(List<BrokenRule> brokenRules)
        {
            BrokenRules = brokenRules;
        }

        public List<T>? Data {get;}
        public List<BrokenRule> BrokenRules {get; } = new();

    }
}
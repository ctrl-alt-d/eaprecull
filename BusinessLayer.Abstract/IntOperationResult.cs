using System;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using CommonInterfaces;

namespace BusinessLayer.Abstract
{

    public class IntOperationResult
    {
        public IntOperationResult(List<BrokenRule> brokenRules)
        {
            BrokenRules = brokenRules;
        }

        public IntOperationResult(int data)
        {
            Data = data;
        }

        public int? Data { get; }
        public List<BrokenRule> BrokenRules { get; } = new();

    }
}

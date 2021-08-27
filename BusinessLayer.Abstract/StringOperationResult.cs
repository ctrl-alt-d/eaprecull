using System;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using CommonInterfaces;

namespace BusinessLayer.Abstract
{

    public class StringOperationResult
    {
        public StringOperationResult(List<BrokenRule> brokenRules)
        {
            BrokenRules = brokenRules;
        }

        public StringOperationResult(string data)
        {
            Data = data;
        }

        public string? Data {get;}
        public List<BrokenRule> BrokenRules {get; } = new();

    }
}

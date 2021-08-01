using System;
using System.Collections.Generic;

namespace BusinessLayer.Abstract.Exceptions

{
    public class BrokenRuleException : Exception
    {
        public BrokenRuleException()
        {
        }

        public BrokenRuleException(string? message, Exception? innerException = null) : base(message, innerException)
        {
            BrokenRules = new() {new BrokenRule(message ?? "")};
        }

        public List<BrokenRule> BrokenRules {get;} = new();

    }
}
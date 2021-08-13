

using System;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public class RuleCheckerItem
    {        
        public RuleCheckerItem(Func<bool> predicate, string? message = null)
        {
            Predicate = predicate;
            Message = message ?? "Error";
        }

        public RuleCheckerItem(Func<Task<bool>> predicate, string? message = null)
        {
            PredicateAsync = predicate;
            Message = message ?? "Error";
        }

        public Func<bool>? Predicate {get; }
        public Func<Task<bool>>? PredicateAsync {get; }
        public string Message {get;}
        public bool IsAsync => PredicateAsync != null;
    }
}
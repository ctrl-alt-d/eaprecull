

using System;
using System.Threading.Tasks;
using DTO.i;

namespace BusinessLayer.Common
{
    public class RuleCheckerItem<T> where T: IDtoi
    {        
        public RuleCheckerItem(Func<T, bool> predicate, string? message = null)
        {
            Predicate = predicate;
            Message = message ?? "Error";
        }

        public RuleCheckerItem(Func<T, Task<bool>> predicate, string? message = null)
        {
            PredicateAsync = predicate;
            Message = message ?? "Error";
        }

        public Func<T, bool>? Predicate {get; }
        public Func<T, Task<bool>>? PredicateAsync {get; }
        public string Message {get;}
        public bool IsAsync => PredicateAsync != null;
    }

    public class RuleCheckerItem<M, T> where T: IDtoi
    {        
        public RuleCheckerItem(Func<M, T, bool> predicate, string? message = null)
        {
            Predicate = predicate;
            Message = message ?? "Error";
        }

        public RuleCheckerItem(Func<M, T, Task<bool>> predicate, string? message = null)
        {
            PredicateAsync = predicate;
            Message = message ?? "Error";
        }

        public Func<M, T, bool>? Predicate {get; }
        public Func<M, T, Task<bool>>? PredicateAsync {get; }
        public string Message {get;}
        public bool IsAsync => PredicateAsync != null;
    }
}
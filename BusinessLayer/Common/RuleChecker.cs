

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using DTO.i;

namespace BusinessLayer.Common
{
    public class RuleChecker<T>: List<RuleCheckerItem<T>> where T: IDtoi
    {
        public RuleChecker(T parms)
        {
            Parms = parms;
        }

        protected T Parms {get;}
        public async Task Check()
        {
            foreach (var item in this)
            {
                if (!item.IsAsync && item.Predicate!(Parms)) 
                    throw new BrokenRuleException(item.Message);

                if (item.IsAsync && (await item.PredicateAsync!(Parms)))
                    throw new BrokenRuleException(item.Message) ; 
            }
        }

        public void Add(Func<T, bool> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem<T>(predicate, message));

        public void Add(Func<T, Task<bool>> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem<T>(predicate, message));

    }

        public class RuleChecker<M, T>: List<RuleCheckerItem<M, T>> where T: IDtoi
    {
        public RuleChecker(M model, T parms)
        {
            Model = model;
            Parms = parms;
        }

        protected T Parms {get;}
        protected M Model {get;}
        public async Task Check()
        {
            foreach (var item in this)
            {
                if (!item.IsAsync && item.Predicate!(Model, Parms)) 
                    throw new BrokenRuleException(item.Message);

                if (item.IsAsync && (await item.PredicateAsync!(Model, Parms)))
                    throw new BrokenRuleException(item.Message) ; 
            }
        }

        public void Add(Func<M, T, bool> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem<M, T>(predicate, message));

        public void Add(Func<M, T, Task<bool>> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem<M, T>(predicate, message));

    }
}
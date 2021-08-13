

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;

namespace BusinessLayer.Common
{
    public class RuleChecker: List<RuleCheckerItem>
    {
        public async Task Check()
        {
            foreach (var item in this)
            {
                if (!item.IsAsync && item.Predicate!()) throw new BrokenRuleException(item.Message) ;
                if (item.IsAsync && (await item.PredicateAsync!())) throw new BrokenRuleException(item.Message) ; 
            }
        }

        public void Add(Func<bool> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem(predicate, message));

        public void Add(Func<Task<bool>> predicate, string? message = null) 
            =>
            this.Add(new RuleCheckerItem(predicate, message));

    }
}
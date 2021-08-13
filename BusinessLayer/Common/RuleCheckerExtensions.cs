

using System;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public static class RuleCheckerExtensions
    {
        public static RuleChecker AddCheck(this RuleChecker ruleChecker, Func<bool> predicate, string? message = null)
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }
        public static RuleChecker AddCheck(this RuleChecker ruleChecker, Func<Task<bool>> predicate, string? message = null)
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }
    }
}
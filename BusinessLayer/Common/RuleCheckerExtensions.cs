

using System;
using System.Threading.Tasks;
using DTO.i;

namespace BusinessLayer.Common
{
    public static class RuleCheckerExtensions
    {
        public static RuleChecker<T> AddCheck<T>(this RuleChecker<T> ruleChecker, Func<T, bool> predicate, string? message = null) where T : IDtoi
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }
        public static RuleChecker<T> AddCheck<T>(this RuleChecker<T> ruleChecker, Func<T, Task<bool>> predicate, string? message = null) where T : IDtoi
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }

        public static RuleChecker<M, T> AddCheck<M, T>(this RuleChecker<M, T> ruleChecker, Func<M, T, bool> predicate, string? message = null) where T : IDtoi
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }
        public static RuleChecker<M, T> AddCheck<M, T>(this RuleChecker<M, T> ruleChecker, Func<M, T, Task<bool>> predicate, string? message = null) where T : IDtoi
        {
            ruleChecker.Add(predicate, message);
            return ruleChecker;
        }

    }
}
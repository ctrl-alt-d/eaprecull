using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using CommonInterfaces;
using DataLayer;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Classe base per a operacions batch que modifiquen múltiples registres.
    /// Exemples: sincronitzar estats, actualitzacions massives, etc.
    /// </summary>
    /// <typeparam name="TResult">Tipus del DTO de resultat</typeparam>
    public abstract class BLBatchOperation<TResult> : BLOperation
        where TResult : IDTOo, IEtiquetaDescripcio
    {
        protected BLBatchOperation(IDbContextFactory<AppDbContext> appDbContextFactory)
            : base(appDbContextFactory)
        {
        }

        /// <summary>
        /// Executa l'operació batch i retorna el resultat.
        /// </summary>
        protected async Task<OperationResult<TResult>> ExecuteBatch(Func<Task<TResult>> batchOperation)
        {
            try
            {
                var result = await batchOperation();
                return new OperationResult<TResult>(result);
            }
            catch (BrokenRuleException br)
            {
                return new OperationResult<TResult>(br.BrokenRules);
            }
            catch (Exception e)
            {
                var brokenRules = new List<BrokenRule> { new BrokenRule($"Error en l'operació batch: {e.Message}") };
                return new OperationResult<TResult>(brokenRules);
            }
        }
    }
}

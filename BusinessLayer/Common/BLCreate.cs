using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DTO.i;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Serilog;

namespace BusinessLayer.Common
{
    public abstract class BLCreate<TModel, TParm, TDTOo>
        : BLOperation,
         ICreate<TDTOo, TParm>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
            where TModel : class, IModel, IId

    {
        public BLCreate(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetCollection()
            =>
            GetContext()
            .Set<TModel>();


        protected abstract Task PreInitialize(TParm parm);
        protected abstract Task<TModel> InitializeModel(TParm parm);
        protected abstract Task PostAdd(TModel model, TParm parm);
        protected abstract Expression<Func<TModel, TDTOo>> ToDto { get; }
        public virtual async Task<OperationResult<TDTOo>> Create(
            TParm parm
            )
        {
            try
            {
                //
                await PreInitialize(parm);
                //
                var model = await InitializeModel(parm);
                GetContext().Add(model);
                //
                await PostAdd(model, parm);
                //
                await GetContext().SaveChangesAsync();
                //
                var dto = await Model2Dto(model);
                //
                return new(dto);
            }
            catch (BrokenRuleException br)
            {
                return new OperationResult<TDTOo>(br.BrokenRules);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error intern no esperat (Create)");
                var bre = new BrokenRuleException($"Error intern no esperat {e.Message}", e);
                return new OperationResult<TDTOo>(bre.BrokenRules);
            }
        }

        protected virtual Task<TDTOo> Model2Dto(TModel model)
            =>
            GetCollection()
            .Where(x => x.Id == model.Id)
            .Select(ToDto)
            .FirstAsync();

    }
}
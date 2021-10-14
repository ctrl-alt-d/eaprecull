using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.i;
using DTO.i.Interfaces;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLSet<TModel, TParm, TDTOo>
        : BLOperation,
         ISet<TParm, TDTOo>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
            where TModel : class, IModel, IId

    {
        public BLSet(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetAllModels()
            =>
            GetContext()
            .Set<TModel>();

        protected abstract IQueryable<TModel> GetModels(TParm request);

        protected abstract Expression<Func<TModel, TDTOo>> ToDto { get; }

        public virtual async Task<OperationResults<TDTOo>> FromPredicate(
            TParm request
            )
        {

            var take = 2000;
            var skip = 0;

            if (request is IPaginated paginatedrequest)
            {
                take = paginatedrequest.Take;
                skip = paginatedrequest.Skip;
            }

            var total =
                await
                GetModels(request)
                .CountAsync();

            var data =
                await
                GetModels(request)
                .Skip(skip)
                .Take(take)
                .Select(ToDto)
                .ToListAsync();

            return new OperationResults<TDTOo>(data, total, take);
        }

        public virtual async Task<IntOperationResult> CountFromPredicate(
            TParm request
            )
        {
            var total =
                await
                GetModels(request)
                .CountAsync();

            return new IntOperationResult(total);
        }

        public virtual async Task<OperationResult<TDTOo>> FromId(int id)
        {
            var data =
                await
                GetAllModels()
                .Where(x => x.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            return new OperationResult<TDTOo>(data);
        }

    }
}
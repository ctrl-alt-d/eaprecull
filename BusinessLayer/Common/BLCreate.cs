using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.i;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLCreate<TModel, TParm, TDTOo>
        :BLOperation,
         ICreate<TDTOo, TParm>
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TParm: IDtoi
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


        protected abstract Task PreInitialize(TParm parm );
        protected abstract Task<TModel> InitializeModel(TParm parm );
        protected abstract Task PostInitialize(TModel model, TParm parm );
        protected abstract TDTOo ToDto(TModel parm );

        public virtual async Task<OperationResult<TDTOo>> Create(
            TParm parm
            )
            {
                //
                await PreInitialize(parm);
                //
                var model = await InitializeModel(parm);
                GetContext().Add(model);                
                //
                await GetContext().SaveChangesAsync();
                //
                await PostInitialize(model, parm);
                //
                return new( ToDto(model) );
            }
    }
}
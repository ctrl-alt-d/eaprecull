using BusinessLayer.Abstract;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BusinessLayer.Common
{
    public class BLGetItem<TModel, TDTOo> : BLOperation
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TModel: class, IModel, IId

    {
        public BLGetItem(IDbContextFactory<AppDbContext> appDbContextFactory)
        :base(appDbContextFactory)
        {
        }

        protected virtual TModel GetById(int id) 
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>()
            .Find(id);


        public virtual OperationResult<TDTOo> Get(
            Func<TModel, TDTOo> toDto,
            int id
            ) 
            =>
            new (
                toDto( GetById(id) )
            );
        
    }
}
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
    public class BLGetItems<TModel, TDTOo> : BLOperation
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TModel: class, IModel, IId

    {
        public BLGetItems(IDbContextFactory<AppDbContext> appDbContextFactory)
        :base(appDbContextFactory)
        {
        }

        public virtual IQueryable<TModel> GetAllModels() 
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>();


        public virtual OperationResults<TDTOo> Execute(
            Func<TModel, TDTOo> toDto,
            IQueryable<TModel>? get
            ) 
            =>
            new (
                (get ?? GetAllModels())
                .Select(toDto)
                .ToList()
            );
        
    }
}
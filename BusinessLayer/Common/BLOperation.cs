using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Common
{
    public class BLOperation
    {
        protected readonly IDbContextFactory<AppDbContext> AppDbContextFactory;

        public BLOperation(IDbContextFactory<AppDbContext> appDbContextFactory)
        {
            AppDbContextFactory = appDbContextFactory;
        }
    }
}
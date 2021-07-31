using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Common
{
    public class BLOperation
    {
        public readonly IDbContextFactory<AppDbContext> AppDbContextFactory;

        public BLOperation(IDbContextFactory<AppDbContext> appDbContextFactory)
        {
            AppDbContextFactory = appDbContextFactory;
        }
    }
}
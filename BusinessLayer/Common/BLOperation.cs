using DataLayer;

namespace BusinessLayer.Common
{
    public class BLOperation
    {
        protected readonly AppDbContext Ctx;

        public BLOperation(AppDbContext ctx)
        {
            Ctx = ctx;
        }
    }
}
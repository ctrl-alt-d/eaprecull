using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class AlumneSyncActiuByCentre : BLBatchOperation<EtiquetaDescripcio>, IAlumneSyncActiuByCentre
    {
        public AlumneSyncActiuByCentre(IDbContextFactory<AppDbContext> appDbContextFactory)
            : base(appDbContextFactory)
        {
        }

        public Task<OperationResult<EtiquetaDescripcio>> Run()
            => ExecuteBatch(SyncAlumnes);

        private async Task<EtiquetaDescripcio> SyncAlumnes()
        {
            var ctx = GetContext();

            var alumnes =
                await
                ctx
                .Alumnes
                .Include(a => a.CentreActual)
                .Where(a =>
                    a.CentreActual == null ||
                    a.EsActiu != a.CentreActual!.EsActiu
                )
                .ToListAsync();

            alumnes
                .ForEach(a => a.EsActiu = a.CentreActual?.EsActiu ?? true);

            var n = await ctx.SaveChangesAsync();

            return new EtiquetaDescripcio(
                etiqueta: $"{n} alumnes canviats d'estat 'Actiu'",
                descripcio: "S'ha sincronitzat l'estat 'Actiu' de centre amb l'estat 'Actiu' d'alumne"
            );
        }
    }
}

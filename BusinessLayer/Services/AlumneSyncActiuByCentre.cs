using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DataModels.Models;
using dtoo = DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;
using SharpDocx;
using DTO.o.DTOs;

namespace BusinessLayer.Services
{

    public class AlumneSyncActiuByCentre : BLOperation, IAlumneSyncActiuByCentre
    {

        public AlumneSyncActiuByCentre(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        public async Task<OperationResult<EtiquetaDescripcio>> Run()
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

            var data = new EtiquetaDescripcio(
                etiqueta: $"{n} alumnes canviats d'estat 'Actiu'",
                descripcio: "S'ha sincronitzat l'estat 'Actiu' de centre amb l'estat 'Actiu' d'alumne"
            );

            return new(data);

        }
    }
}
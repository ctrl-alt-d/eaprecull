using System;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using DTO.Projections;
using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class ActuacioByAlumne : BLOperation, IActuacioByAlumne
    {
        public ActuacioByAlumne(IDbContextFactory<AppDbContext> appDbContextFactory)
        :base(appDbContextFactory)
        {
        }

        public OperationResults<dtoo.Actuacio> Query(GetActuacioByAlumneParms request)
            =>
            new (
                AppDbContextFactory
                .CreateDbContext()
                .Actuacions
                // .Include(x=>x.TipusActuacio)
                // .Include(x=>x.CursActuacio)
                // .Include(x=>x.CentreAlMomentDeLactuacio)
                // .Include(x=>x.EtapaAlMomentDeLactuacio)
                .Where(x=>x.Alumne.Id == request.IdAlumne)
                .Select(x=> x.ToDto())
                .ToList()
            );
    }
}

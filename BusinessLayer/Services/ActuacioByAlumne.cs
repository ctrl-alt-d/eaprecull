using System;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using DTO.Projections;

namespace BusinessLayer.Services
{
    public class ActuacioByAlumne : BLOperation, IActuacioByAlumne
    {
        public ActuacioByAlumne(AppDbContext ctx) : base(ctx)
        {
        }

        public OperationResults<dtoo.Actuacio> Query(GetActuacioByAlumneParms request)
            =>
            new (
                Ctx
                .Actuacions
                .Where(x=>x.Alumne.Id == request.IdAlumne)
                .Select(x=> x.ToDto())
                .ToList()
            );
    }
}

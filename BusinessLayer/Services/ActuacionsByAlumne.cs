using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using DTO.Projections;
using models = DataModels.Models;

namespace BusinessLayer.Services
{
    public class ActuacionsByAlumne : IActuacionsByAlumne
    {

        protected readonly BLGetItems<models.Actuacio, dtoo.Actuacio> BLo;
        public ActuacionsByAlumne(BLGetItems<models.Actuacio, dtoo.Actuacio> blo)
        {
            BLo = blo;
        }

        public OperationResults<dtoo.Actuacio> Query(GetActuacioByAlumneParms request)
            =>
            BLo
            .Execute(
                Actuacio.ToDto,
                GetModels(request)
            );

        private IQueryable<models.Actuacio> GetModels(GetActuacioByAlumneParms request) 
            =>
            BLo
            .GetAllModels()
            .Where(x => x.Alumne.Id == request.IdAlumne);

    }
}

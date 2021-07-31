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
    public class Centres : ICentres
    {

        protected readonly BLGetItems<models.Centre, dtoo.Centre> BLo;
        public Centres(BLGetItems<models.Centre, dtoo.Centre> blo)
        {
            BLo = blo;
        }

        public OperationResults<dtoo.Centre> Query(EsActiuParms request)
            =>
            BLo
            .Execute(
                Centre.ToDto,
                GetModels(request)
            );

        private IQueryable<models.Centre> GetModels(EsActiuParms request)
            =>
            BLo
            .GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c=>c.Nom);
    }
}

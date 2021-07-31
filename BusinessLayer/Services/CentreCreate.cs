using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using System.Linq;
using DTO.Projections;
using models = DataModels.Models;
using System.Threading.Tasks;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;

namespace BusinessLayer.Services
{
    public class CentreCreate : BLCreate<models.Centre, parms.CentreCreateParms, dtoo.Centre>
    {
        public CentreCreate(IDbContextFactory<AppDbContext> appDbContextFactory) : base(appDbContextFactory)
        {
        }

        public override models.Centre SetValues(CentreCreateParms parm)
        {
            return new ()
            {
                Codi = parm.Codi,
                Nom = parm.Nom,
                EsActiu = parm.EsActiu,
            };
        }
    }
}

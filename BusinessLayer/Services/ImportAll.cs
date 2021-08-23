using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using parms = DTO.i.DTOs;
using dtoo = DTO.o.DTOs;
using project = DTO.Projections;
using models = DataModels.Models;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using DataModels.Models;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;
using System.Linq;
using BusinessLayer.Abstract;
using DTO.o.DTOs;
using ClosedXML.Excel;
using BusinessLayer.Services.ImportAllHelpers;
using System.Collections.Generic;

namespace BusinessLayer.Services
{
    public class ImportAll : BLOperation, IImportAll
    {

        public ImportAll(
            IDbContextFactory<AppDbContext> appDbContextFactory, 
            ICentreCreate blCentre, ITipusActuacioCreate blTipusActuacio, IEtapaCreate blEtapa, ICursAcademicCreate blCursAcademic, IAlumneCreate blAlumne)
            : base(appDbContextFactory)
        {
            this.blCentre = blCentre;
            this.blTipusActuacio = blTipusActuacio;
            this.blEtapa = blEtapa;
            this.blCursAcademic = blCursAcademic;
            this.blAlumne = blAlumne;
        }

        protected virtual ICentreCreate blCentre {get;}
        protected virtual ITipusActuacioCreate blTipusActuacio {get;}
        protected virtual IEtapaCreate blEtapa {get;}
        protected virtual ICursAcademicCreate blCursAcademic {get;}
        protected virtual IAlumneCreate blAlumne {get;}

        public async Task<OperationResult<ImportAllResult>> Run(string path)
        {
            var result = new ImportAllResult();

            var extractor = new ExtractData(path);
            var data = extractor.Run();

            // Centres
            var dictCentre = await ExtreureCentres(result, data);

            // Cursos

            // Etapes

            // Tipus Actuacio

            // Alumnes

            // Actuacions

            return new OperationResult<ImportAllResult>(result);

        }

        private async Task< Dictionary<string, dtoo.Centre> > ExtreureCentres(ImportAllResult result, List<ActuacioDataRow> data)
        {
            Dictionary<string, dtoo.Centre> dictCentre = new();

            var centreActius =
                data
                .Select(x => x.CentreActual)
                .Distinct()
                .ToList();

            foreach (var item in centreActius)
            {
                var parms = new CentreCreateParms(
                    codi: "**codi**",
                    nom: item,
                    esActiu: true
                );
                var dto = await blCentre.Create(parms);
                dictCentre.Add(item, dto.Data!);
                result.NumCentres++;
            }

            var centreNoActius =
                data
                .Select(x => x.CentreActuacio)
                .Where(x => !centreActius.Contains(x))
                .Distinct()
                .ToList();

            foreach (var item in centreNoActius)
            {
                var parms = new CentreCreateParms(
                    codi: "**codi**",
                    nom: item,
                    esActiu: false
                );
                var dto = await blCentre.Create(parms);
                dictCentre.Add(item, dto.Data!);
                result.NumCentres++;
            }

            return dictCentre;
        }
    }
}

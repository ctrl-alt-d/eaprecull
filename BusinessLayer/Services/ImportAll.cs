﻿using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using dtoo = DTO.o.DTOs;
using DTO.i.DTOs;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Threading.Tasks;
using System;
using System.Linq;
using BusinessLayer.Abstract;
using DTO.o.DTOs;
using BusinessLayer.Services.ImportAllHelpers;
using System.Collections.Generic;

namespace BusinessLayer.Services
{
    public class ImportAll : BLOperation, IImportAll
    {

        public readonly string ETAPAPERDEFECTE = "Estudiant";

        public ImportAll(
            IDbContextFactory<AppDbContext> appDbContextFactory,
            ICentreCreate blCentre, ITipusActuacioCreate blTipusActuacio, IEtapaCreate blEtapa, ICursAcademicCreate blCursAcademic, IAlumneCreate blAlumne, IActuacioCreate blActuacio)
            : base(appDbContextFactory)
        {
            this.blCentre = blCentre;
            this.blTipusActuacio = blTipusActuacio;
            this.blEtapa = blEtapa;
            this.blCursAcademic = blCursAcademic;
            this.blAlumne = blAlumne;
            this.blActuacio = blActuacio;
        }

        protected virtual ICentreCreate blCentre { get; }
        protected virtual ITipusActuacioCreate blTipusActuacio { get; }
        protected virtual IEtapaCreate blEtapa { get; }
        protected virtual ICursAcademicCreate blCursAcademic { get; }
        protected virtual IAlumneCreate blAlumne { get; }
        protected virtual IActuacioCreate blActuacio { get; }

        public async Task<OperationResult<ImportAllResult>> Run(string path)
        {
            //
            var result = new ImportAllResult();

            //
            var data = new ExtractData(path).Run();

            // Centres
            var dictCentre = await ExtreureCentres(result, data);

            // Cursos
            var dictCursos = await ExtreureCursos(result, data);

            // Etapes
            var dictEtapes = await ExtreureEtapes(result, data);

            // Tipus Actuacio
            var dictTipusActuacio = await ExtreureTipusActuacio(result, data);

            // Alumnes
            var dictAlumne = await ExtreureAlumne(result, data, dictCentre, dictCursos, dictEtapes, dictTipusActuacio);

            // Actuacions
            var dictActuacions = await ExtreuActuacions(result, data, dictCentre, dictCursos, dictEtapes, dictTipusActuacio, dictAlumne);

            await DarreraModificacioAlumnes();

            return new OperationResult<ImportAllResult>(result);

        }

        private async Task DarreraModificacioAlumnes()
        {
            var data =
                GetContext()
                .Alumnes
                .Select(x => new {alumne = x, darreraModificacio = x.Actuacions.Select(a=>a.MomentDeLactuacio).OrderByDescending(a=>a).First()}) 
                .ToList();

            foreach(var item in data)
            {
                item.alumne.DataDarreraModificacio = item.darreraModificacio;
            }

            await GetContext().SaveChangesAsync();
        }

        private async Task<Dictionary<string, dtoo.Centre>> ExtreureCentres(ImportAllResult result, List<ActuacioDataRow> data)
        {
            Dictionary<string, dtoo.Centre> d = new();

            var itemsActius =
                data
                .Select(x => x.CentreActual)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            foreach (var item in itemsActius)
            {
                var parms = new CentreCreateParms(
                    codi: item,
                    nom: item,
                    esActiu: true
                );
                var dto = await blCentre.Create(parms);
                d.Add(item, dto.Data!);
                result.NumCentres++;
            }

            var itemsNoActius =
                data
                .Select(x => x.CentreActuacio)
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(x => !itemsActius.Contains(x))
                .Distinct()
                .ToList();

            foreach (var item in itemsNoActius)
            {
                var parms = new CentreCreateParms(
                    codi: item,
                    nom: item,
                    esActiu: false
                );
                var dto = await blCentre.Create(parms);
                d.Add(item, dto.Data!);
                result.NumCentres++;
            }

            return d;
        }

        private async Task<Dictionary<string, dtoo.CursAcademic>> ExtreureCursos(ImportAllResult result, List<ActuacioDataRow> data)
        {
            Dictionary<string, dtoo.CursAcademic> d = new();

            var items =
                data
                .Select(x => x.CursActuacio)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => (anyTxt: x.Substring(0, 4), curs: x))
                .Select(x => (anyInici: Convert.ToInt32(x.anyTxt), x.curs))
                .Distinct()
                .OrderBy(x => x.anyInici)
                .ToList();

            var darrer = items.Last();

            foreach (var item in items)
            {
                var parms = new CursAcademicCreateParms(
                    anyInici: item.anyInici,
                    esActiu: item.anyInici == darrer.anyInici
                );
                var dto = await blCursAcademic.Create(parms);
                d.Add(item.curs, dto.Data!);
                result.NumCursosAcademics++;
            }

            return d;
        }

        private async Task<Dictionary<string, dtoo.Etapa>> ExtreureEtapes(ImportAllResult result, List<ActuacioDataRow> data)
        {
            Dictionary<string, dtoo.Etapa> d = new();

            var etapesActuacio =
                data
                .Select(x => x.EtapaActuacio);

            var etapesActual =
                data
                .Select(x => x.EtapaActual);

            var items =
                Array.Empty<string>()
                .Concat(etapesActuacio)
                .Concat(etapesActual)
                .Distinct()
                .ToList();

            foreach (var item in items)
            {
                var codi = string.IsNullOrEmpty(item) ? this.ETAPAPERDEFECTE : item;
                var parms = new EtapaCreateParms(
                    codi: codi,
                    nom: codi,
                    sonEstudisObligatoris: false,
                    esActiu: item != this.ETAPAPERDEFECTE
                );
                var dto = await blEtapa.Create(parms);
                d.Add(codi, dto.Data!);
                result.NumEtapes++;
            }

            return d;
        }

        private async Task<Dictionary<string, dtoo.TipusActuacio>> ExtreureTipusActuacio(ImportAllResult result, List<ActuacioDataRow> data)
        {
            Dictionary<string, dtoo.TipusActuacio> d = new();

            var items =
                data
                .Select(x => x.TipusActuacio)
                .Distinct()
                .ToList();

            foreach (var item in items)
            {
                var parms = new TipusActuacioCreateParms(
                    codi: item,
                    nom: item,
                    esActiu: true
                );
                var dto = await blTipusActuacio.Create(parms);
                d.Add(item, dto.Data!);
                result.NumTipusActuacio++;
            }

            return d;
        }

        private async Task<Dictionary<string, dtoo.Alumne>> ExtreureAlumne(
            ImportAllResult result, List<ActuacioDataRow> data,
            Dictionary<string, dtoo.Centre> dictCentre, Dictionary<string, dtoo.CursAcademic> dictCursos, Dictionary<string, dtoo.Etapa> dictEtapes, Dictionary<string, dtoo.TipusActuacio> dictTipusActuacio
        )
        {
            Dictionary<string, dtoo.Alumne> d = new();

            var cursActual = dictCursos.Select(d=>(d.Key, d.Value)).OrderByDescending(t=>t.Value.AnyInici).Select(t=>t.Key).First();

            var items =
                data
                .GroupBy(x =>                     
                    x.Nom + x.Cognoms + x.DataNaixement.ToString(),                    
                    (k,l) => (codi: k, dto: l.OrderByDescending(x=>x.MomentDeLactuacio).First())
                )
                .Distinct()
                .ToList();

            foreach (var item in items)
            {
                var v = item.dto;

                var esCursActual = v.CursActuacio == cursActual;

                var centreActual = !string.IsNullOrWhiteSpace( v.CentreActual ) ? v.CentreActual : v.CentreActuacio;
                var centreId = dictCentre.GetValueOrDefault(centreActual)?.Id;

                var etapaActual = !string.IsNullOrWhiteSpace( v.EtapaActual ) ? v.EtapaActual : v.EtapaActuacio;
                var etapaId = dictEtapes.GetValueOrDefault(etapaActual)?.Id;

                var cursActuacioId = dictCursos[v.CursActuacio].Id;

                var nivellActual = !string.IsNullOrWhiteSpace(v.NivellActual) ? v.NivellActual : v.NivellActuacio;


                var parms = new AlumneCreateParms(
                    nom: v.Nom,
                    cognoms: v.Cognoms,
                    dataNaixement: v.DataNaixement,
                    centreActualId: centreId,
                    cursDarreraActualitacioDadesId: cursActuacioId,
                    etapaActualId: etapaId,
                    nivellActual: nivellActual,
                    dataInformeNESENEE: v.DataInformeNESENEE,
                    observacionsNESENEE: v.ObservacionsNESENEE,
                    dataInformeNESENoNEE: v.DataInformeNESENoNEE,
                    observacionsNESENoNEE: v.ObservacionsNESENoNEE,
                    tags: $"#importateapactua{DateTime.Now.Year}"
                );
                var dto = await blAlumne.Create(parms);
                d.Add(item.codi, dto.Data!);
                result.NumAlumnes++;
            }

            return d;            
        }

        private async Task<Dictionary<string, dtoo.Actuacio>> ExtreuActuacions(
            ImportAllResult result, List<ActuacioDataRow> data, 
            Dictionary<string, dtoo.Centre> dictCentre, Dictionary<string, dtoo.CursAcademic> dictCursos, Dictionary<string, dtoo.Etapa> dictEtapes, Dictionary<string, dtoo.TipusActuacio> dictTipusActuacio, Dictionary<string, dtoo.Alumne> dictAlumne)
        {
            Dictionary<string, dtoo.Actuacio> d = new();

            var cursActual = dictCursos.Select(d=>(d.Key, d.Value)).OrderByDescending(t=>t.Value.AnyInici).Select(t=>t.Key).First();

            var items =
                data
                .Select(x => (
                     codi: x.MomentDeLactuacio.ToString() +  x.Nom + x.Cognoms + x.DataNaixement.ToString(),
                     dto: x,
                     alumne: x.Nom + x.Cognoms + x.DataNaixement.ToString()
                     )
                )
                .ToList();

            foreach (var item in items)
            {
                var v = item.dto;

                //
                var esCursActual = v.CursActuacio == cursActual;

                //
                var centreActuacioId = dictCentre[v.CentreActuacio].Id;

                //
                var etapaActuacioId = ( dictEtapes.GetValueOrDefault(v.EtapaActuacio) ?? dictEtapes[ETAPAPERDEFECTE]).Id;

                //
                var cursActuacioId = dictCursos[v.CursActuacio].Id;

                //
                var nivellActual = !string.IsNullOrWhiteSpace(v.NivellActual) ? v.NivellActual : v.NivellActuacio;

                //
                var alumneId = dictAlumne[item.alumne].Id;

                //
                var tipusActuacioId = dictTipusActuacio[v.TipusActuacio].Id;

                //
                var parms = new ActuacioCreateParms(
                    alumneId: alumneId,
                    tipusActuacioId: tipusActuacioId,
                    observacionsTipusActuacio: v.ObservacionsTipusActuacio,
                    momentDeLactuacio: v.MomentDeLactuacio,
                    cursActuacioId: cursActuacioId,
                    centreAlMomentDeLactuacioId: centreActuacioId,
                    etapaAlMomentDeLactuacioId: etapaActuacioId,
                    nivellAlMomentDeLactuacio: v.NivellActuacio,
                    minutsDuradaActuacio: v.DuradaActuacio,
                    descripcioActuacio: v.DescripcioActuacio 
                );
                var dto = await blActuacio.Create(parms);
                d.Add(item.codi, dto.Data!);

                //
                result.NumAlumnes++;
            }

            return d;          
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using Dtoo = DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table.PivotTable;

namespace BusinessLayer.Services
{
    public class PivotActuacions : BLReport<Dtoo.SaveResult>, IPivotActuacions
    {
        internal record DataRecord(string Curs, string Alumne, string Data, string Centre, string Tipus, int Minuts, string Etapa, string EstudisObligatoris);

        public PivotActuacions(IDbContextFactory<AppDbContext> appDbContextFactory) 
            : base(appDbContextFactory)
        {
            ExcelPackage.License.SetNonCommercialOrganization("EAPRecull - Educational Use");
        }

        public Task<OperationResult<Dtoo.SaveResult>> Run()
            => ExecuteReport(GenerateReport);

        private async Task<Dtoo.SaveResult> GenerateReport()
        {
            var dades = await GetInformes();
            var (path, filename, folder) = CalculatePath("pivot_actuacions", "xlsx");
            
            ExportaExcel(path, dades);

            return new Dtoo.SaveResult(path, filename, folder);
        }

        private void ExportaExcel(string path, List<DataRecord> dades)
        {
            // https://github.com/EPPlusSoftware/EPPlus/wiki/Pivot-Tables
            // https://github.com/EPPlusSoftware/EPPlus.Sample.NetCore/blob/master/18-PivotTables/PivotTablesSample.cs
            using var pck = new ExcelPackage();
            var wsData=pck.Workbook.Worksheets.Add("Dades");
            //
            var dataRange = wsData.Cells["A1"].LoadFromCollection(
                    dades,
                    PrintHeaders: true,
                    TableStyle: OfficeOpenXml.Table.TableStyles.Medium2);

            wsData.Cells[2, 3, dataRange.End.Row, 3].Style.Numberformat.Format = "dd-mm-yy";
            dataRange.AutoFitColumns();

            //
            // - pivot
            //
            var wsPivot = pck.Workbook.Worksheets.Add("Pivot");
            var pivotTable1 = wsPivot.PivotTables.Add(wsPivot.Cells["A5"], dataRange, "Actuacions");

            // - page
            var pageField = pivotTable1.PageFields.Add(pivotTable1.Fields["Curs"]);
            pageField.Items.Refresh();  
            pageField.Items.SelectSingleItem(0);

            // - rows
            pivotTable1.RowFields.Add(pivotTable1.Fields["Centre"]);
            pivotTable1.RowFields.Add(pivotTable1.Fields["Tipus"]);

            // - data
            pivotTable1.DataOnRows = false;

            var dataFieldMinuts = pivotTable1.DataFields.Add(pivotTable1.Fields["Minuts"]);
            dataFieldMinuts.Format="#,##0";
            dataFieldMinuts.Function = DataFieldFunctions.Sum;

            var dataFieldNombre = pivotTable1.DataFields.Add(pivotTable1.Fields["Minuts"]);
            dataFieldNombre.Format="#,##0";
            dataFieldNombre.Function = DataFieldFunctions.Count;

            //
            pck.SaveAs(new FileInfo(path));
        }

        private Task<List<DataRecord>> GetInformes()
            =>
            GetContext()
            .Actuacions
            .Include(a => a.Alumne)
            .Include(a => a.CursActuacio)
            .Include(a => a.CentreAlMomentDeLactuacio)
            .Include(a => a.EtapaAlMomentDeLactuacio)
            .Include(a => a.TipusActuacio)
            .OrderByDescending(a => a.CursActuacio.AnyInici)
            .ThenBy(a => a.CentreAlMomentDeLactuacio.Nom)
            .ThenBy(a => a.Alumne.Cognoms)
            .ThenBy(a => a.Alumne.Nom)
            .ThenBy(a => a.MomentDeLactuacio)
            .AsNoTracking()
            .Select( d => new DataRecord(
                                d.CursActuacio.Nom,
                                $"{d.Alumne.Cognoms}, {d.Alumne.Nom} (#{d.Alumne.Id})",
                                d.MomentDeLactuacio.ToString("yyyy-MM-dd"),
                                d.CentreAlMomentDeLactuacio.Nom,
                                d.TipusActuacio.Nom,
                                d.MinutsDuradaActuacio,
                                d.EtapaAlMomentDeLactuacio.Nom,
                                d.EtapaAlMomentDeLactuacio.SonEstudisObligatoris ? "Sí" : "No"
                )
            )            
            .ToListAsync();
    }
}
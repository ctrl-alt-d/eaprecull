
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;

namespace BusinessLayer.Services.ImportAllHelpers
{

    public class ExtractData
    {
        private readonly string SourceFilePath;
        public ExtractData(string sourceFilePath)
        {
            SourceFilePath = sourceFilePath;
        }

        public List<ActuacioDataRow> Run()
        {
            var dupItems = LoadItems();

            var items = RemoveDups(dupItems);

            return items;
        }

        private static List<ActuacioDataRow> RemoveDups(List<ActuacioDataRow> dupItems)
        {
            return dupItems
                .GroupBy(item =>
                    new { item.DataNaixement, item.Nom, item.Cognoms, item.CursActuacio, item.MomentDeLactuacio, item.DescripcioActuacio },
                    (k, l) => l.First())
                .ToList();
        }

        private List<ActuacioDataRow> LoadItems()
        {

            var items = new List<ActuacioDataRow>();
            var wb = new XLWorkbook(SourceFilePath);
            var ws = wb.Worksheet("Data");
            var firstRowUsed = ws.FirstRowUsed();
            var lastRowUsed = ws.LastRowUsed();

            var row = firstRowUsed.RowUsed().RowBelow();
            while (row != lastRowUsed)
            {

                if (row.Cell((int)ActuacioDataRow.Camps.Nom).IsEmpty())
                    continue;

                var item = new ActuacioDataRow(
                    row.Cell((int)ActuacioDataRow.Camps.Nom).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.Cognoms).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataNaixement).GetDateTime(),
                    row.Cell((int)ActuacioDataRow.Camps.CentreActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.EtapaActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.NuvellActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENoNEE).IsEmpty() ? null : row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENEE).GetDateTime(),
                    row.Cell((int)ActuacioDataRow.Camps.ObservacionsNESENEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENoNEE).IsEmpty() ? null : row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENoNEE).GetDateTime(),
                    row.Cell((int)ActuacioDataRow.Camps.ObservacionsNESENoNEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.TipusActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.MomentDeLactuacio).GetDateTime(),
                    row.Cell((int)ActuacioDataRow.Camps.CursActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.CentreActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.EtapaActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.NivellActuacio).GetString(),
                    Convert.ToInt32(row.Cell((int)ActuacioDataRow.Camps.DuradaActuacio).GetDouble()),
                    row.Cell((int)ActuacioDataRow.Camps.DescripcioActuacio).GetString()
                );
                items.Add(item);
                row = row.RowBelow();
            }

            return items;
        }
    }

}
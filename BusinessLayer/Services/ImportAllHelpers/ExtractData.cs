
using System.Collections.Generic;
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
            var wb = new XLWorkbook(SourceFilePath);
            var ws = wb.Worksheet("Data");

            var firstRowUsed = ws.FirstRowUsed();

            var row = firstRowUsed.RowUsed().RowBelow();

            var items = new List<ActuacioDataRow>();

            // Get all categories
            while (!row.Cell((int)ActuacioDataRow.Camps.Nom).IsEmpty())
            {
                var item = new ActuacioDataRow(
                    row.Cell((int)ActuacioDataRow.Camps.Nom).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.Cognoms).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataNaixement).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.CentreActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.EtapaActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.NuvellActual).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.ObservacionsNESENEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DataInformeNESENoNEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.ObservacionsNESENoNEE).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.TipusActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.MomentDeLactuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.CursActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.CentreActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.EtapaActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.NivellActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DuradaActuacio).GetString(),
                    row.Cell((int)ActuacioDataRow.Camps.DescripcioActuacio).GetString()
                );
                items.Add(item);
                row = row.RowBelow();

            }

            return items;
        }

    }

}
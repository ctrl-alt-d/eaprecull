using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using CommonInterfaces;
using DataLayer;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Classe base per a operacions de generació d'informes/reports.
    /// Els reports generen fitxers (Word, Excel, PDF, etc.) i retornen el path del resultat.
    /// </summary>
    /// <typeparam name="TResult">Tipus del DTO de resultat (normalment SaveResult)</typeparam>
    public abstract class BLReport<TResult> : BLOperation
        where TResult : IDTOo, IEtiquetaDescripcio
    {
        protected BLReport(IDbContextFactory<AppDbContext> appDbContextFactory)
            : base(appDbContextFactory)
        {
        }

        /// <summary>
        /// Executa el report i retorna el resultat.
        /// </summary>
        protected async Task<OperationResult<TResult>> ExecuteReport(Func<Task<TResult>> generateReport)
        {
            try
            {
                var result = await generateReport();
                return new OperationResult<TResult>(result);
            }
            catch (BrokenRuleException br)
            {
                return new OperationResult<TResult>(br.BrokenRules);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error generant l'informe");
                var brokenRules = new List<BrokenRule> { new BrokenRule($"Error generant l'informe: {e.Message}") };
                return new OperationResult<TResult>(brokenRules);
            }
        }

        /// <summary>
        /// Calcula el path de sortida per al report.
        /// </summary>
        /// <param name="prefixFilename">Prefix pel nom del fitxer</param>
        /// <param name="extension">Extensió del fitxer (sense punt)</param>
        /// <returns>Tuple amb path complet, nom del fitxer i carpeta</returns>
        protected static (string path, string filename, string folder) CalculatePath(string prefixFilename, string extension = "docx")
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EapRecullData",
                "Reports"
            );

            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{prefixFilename}_{timestamp}.{extension}";
            var path = System.IO.Path.Combine(folder, filename);

            return (path, filename, folder);
        }

        /// <summary>
        /// Obté el path a les plantilles de reports.
        /// </summary>
        protected static string GetTemplatesPath(string templateFileName)
        {
            return System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Templates",
                templateFileName
            );
        }
    }
}

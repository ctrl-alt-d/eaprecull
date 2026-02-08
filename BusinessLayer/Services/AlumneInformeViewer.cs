using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using DataModels.Models;
using Dtoo = DTO.o.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BusinessLayer.Services
{
    /// <summary>
    /// Servei per obtenir les dades de l'informe d'un alumne per mostrar per pantalla
    /// </summary>
    public class AlumneInformeViewer : BLOperation, IAlumneInformeViewer
    {
        public AlumneInformeViewer(IDbContextFactory<AppDbContext> appDbContextFactory)
            : base(appDbContextFactory)
        {
        }

        public async Task<OperationResult<Dtoo.AlumneInformeViewerData>> Run(int alumneId)
        {
            try
            {
                var alumne = await GetDadesAlumne(alumneId);

                if (alumne == null)
                    throw new BrokenRuleException("Alumne no trobat");

                var cursActiu = await GetCursActiu();
                var data = MapToDto(alumne, cursActiu);
                return new OperationResult<Dtoo.AlumneInformeViewerData>(data);
            }
            catch (BrokenRuleException br)
            {
                return new OperationResult<Dtoo.AlumneInformeViewerData>(br.BrokenRules);
            }
        }

        private Task<Alumne?> GetDadesAlumne(int alumneId)
            =>
            GetContext()
            .Alumnes
            .Include(a => a.Actuacions.OrderByDescending(x => x.MomentDeLactuacio))
            .Include(a => a.Actuacions).ThenInclude(a => a.CursActuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.CentreAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.EtapaAlMomentDeLactuacio)
            .Include(a => a.Actuacions).ThenInclude(a => a.TipusActuacio)
            .Include(a => a.CursDarreraActualitacioDades)
            .Include(a => a.CentreActual)
            .Include(a => a.EtapaActual)
            .Where(a => a.Id == alumneId)
            .FirstOrDefaultAsync();

        private Task<CursAcademic?> GetCursActiu()
            =>
            GetContext()
            .CursosAcademics
            .Where(c => c.EsActiu)
            .FirstOrDefaultAsync();

        private Dtoo.AlumneInformeViewerData MapToDto(Alumne alumne, CursAcademic? cursActiu)
        {
            var actuacions = alumne.Actuacions
                .OrderByDescending(a => a.MomentDeLactuacio)
                .Select(a => new Dtoo.ActuacioInformeItem(
                    a.Id,
                    a.MomentDeLactuacio,
                    a.CursActuacio?.Etiqueta ?? "-",
                    a.TipusActuacio?.Etiqueta ?? "-",
                    a.ObservacionsTipusActuacio,
                    a.CentreAlMomentDeLactuacio?.Etiqueta ?? "-",
                    a.EtapaAlMomentDeLactuacio?.Etiqueta ?? "-",
                    a.NivellAlMomentDeLactuacio,
                    a.MinutsDuradaActuacio,
                    a.DescripcioActuacio
                ))
                .ToList();

            return new Dtoo.AlumneInformeViewerData(
                alumne.Id,
                alumne.Nom,
                alumne.Cognoms,
                alumne.DataNaixement,
                alumne.CursDarreraActualitacioDades?.Etiqueta ?? "-",
                alumne.CentreActual?.Etiqueta ?? "-",
                alumne.EtapaActual?.Etiqueta ?? "-",
                alumne.NivellActual,
                alumne.DataInformeNESENEE,
                alumne.ObservacionsNESENEE,
                alumne.DataInformeNESENoNEE,
                alumne.ObservacionsNESENoNEE,
                alumne.Tags,
                cursActiu != null && alumne.CursDarreraActualitacioDades?.Id != cursActiu.Id,
                actuacions
            );
        }
    }
}

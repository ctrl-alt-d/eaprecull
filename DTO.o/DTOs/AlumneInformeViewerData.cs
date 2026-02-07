using System;
using System.Collections.Generic;
using System.Linq;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    /// <summary>
    /// DTO per mostrar l'informe de l'alumne per pantalla (viewer)
    /// </summary>
    public class AlumneInformeViewerData : IEtiquetaDescripcio, IDTOo
    {
        public AlumneInformeViewerData(
            int id,
            string nom,
            string cognoms,
            DateTime? dataNaixement,
            string centreActual,
            string etapaActual,
            string nivellActual,
            DateTime? dataInformeNESENEE,
            string observacionsNESENEE,
            DateTime? dataInformeNESENoNEE,
            string observacionsNESENoNEE,
            string tags,
            List<ActuacioInformeItem> actuacions)
        {
            Id = id;
            Nom = nom;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActual = centreActual;
            EtapaActual = etapaActual;
            NivellActual = nivellActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            Tags = tags;
            Actuacions = actuacions;
        }

        public int Id { get; }
        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime? DataNaixement { get; }
        public string CentreActual { get; }
        public string EtapaActual { get; }
        public string NivellActual { get; }
        public DateTime? DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public string Tags { get; }
        public List<ActuacioInformeItem> Actuacions { get; }

        // Propietats calculades per la UI
        public string NomComplet => $"{Cognoms}, {Nom}";
        public string DataNaixementTxt => DataNaixement?.ToString("dd/MM/yyyy") ?? "-";
        public bool TeNESENEE => DataInformeNESENEE.HasValue;
        public string DataNESENEETxt => DataInformeNESENEE?.ToString("dd/MM/yyyy") ?? "-";
        public bool TeNESENoNEE => DataInformeNESENoNEE.HasValue;
        public string DataNESENoNEETxt => DataInformeNESENoNEE?.ToString("dd/MM/yyyy") ?? "-";
        public int TotalActuacions => Actuacions.Count;
        
        // Temps total d'actuacions
        public int TotalMinuts => Actuacions.Sum(a => a.MinutsDuradaActuacio);
        public int TotalHores => TotalMinuts / 60;
        public int MinutsRestants => TotalMinuts % 60;
        public string TempsTotalTxt => TotalMinuts > 0 
            ? (TotalHores > 0 
                ? $"{TotalHores}h {MinutsRestants}min" 
                : $"{MinutsRestants}min")
            : "-";

        // IEtiquetaDescripcio
        public string Etiqueta => NomComplet;
        public string Descripcio => $"{CentreActual} - {EtapaActual}";
    }

    /// <summary>
    /// Representa una actuació dins l'informe viewer
    /// </summary>
    public class ActuacioInformeItem
    {
        public ActuacioInformeItem(
            int id,
            DateTime momentDeLactuacio,
            string cursActuacio,
            string tipusActuacio,
            string observacionsTipusActuacio,
            string centreAlMomentDeLactuacio,
            string etapaAlMomentDeLactuacio,
            string nivellAlMomentDeLactuacio,
            int minutsDuradaActuacio,
            string descripcioActuacio)
        {
            Id = id;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            TipusActuacio = tipusActuacio;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            CentreAlMomentDeLactuacio = centreAlMomentDeLactuacio;
            EtapaAlMomentDeLactuacio = etapaAlMomentDeLactuacio;
            NivellAlMomentDeLactuacio = nivellAlMomentDeLactuacio;
            MinutsDuradaActuacio = minutsDuradaActuacio;
            DescripcioActuacio = descripcioActuacio;
        }

        public int Id { get; }
        public DateTime MomentDeLactuacio { get; }
        public string CursActuacio { get; }
        public string TipusActuacio { get; }
        public string ObservacionsTipusActuacio { get; }
        public string CentreAlMomentDeLactuacio { get; }
        public string EtapaAlMomentDeLactuacio { get; }
        public string NivellAlMomentDeLactuacio { get; }
        public int MinutsDuradaActuacio { get; }
        public string DescripcioActuacio { get; }

        // Propietats calculades per la UI
        public string DataTxt => MomentDeLactuacio.ToString("dd/MM/yyyy");
        public string DuradaTxt => MinutsDuradaActuacio > 0 ? $"{MinutsDuradaActuacio} min" : "-";
        public string ContextTxt => $"{CentreAlMomentDeLactuacio} · {EtapaAlMomentDeLactuacio} · {NivellAlMomentDeLactuacio}";
    }
}

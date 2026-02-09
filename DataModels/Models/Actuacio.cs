using System;
using System.Linq;
using CommonInterfaces;

namespace DataModels.Models
{

    public class Actuacio : IIdEtiquetaDescripcio, IModel
    {
        public Actuacio()
        {
        }

        public Actuacio(Alumne alumne, TipusActuacio tipusActuacio, string observacionsTipusActuacio, DateTime momentDeLactuacio, CursAcademic cursActuacio, Centre centreAlMomentDeLactuacio, Etapa etapaAlMomentDeLactuacio, string nivellAlMomentDeLactuacio, int minutsDuradaActuacio, string descripcioActuacio)
        {
            Alumne = alumne;
            TipusActuacio = tipusActuacio;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            CentreAlMomentDeLactuacio = centreAlMomentDeLactuacio;
            EtapaAlMomentDeLactuacio = etapaAlMomentDeLactuacio;
            NivellAlMomentDeLactuacio = nivellAlMomentDeLactuacio;
            MinutsDuradaActuacio = minutsDuradaActuacio;
            DescripcioActuacio = descripcioActuacio;
        }

        public void SetMainData(Alumne alumne, TipusActuacio tipusActuacio, string observacionsTipusActuacio, DateTime momentDeLactuacio, CursAcademic cursActuacio, Centre centreAlMomentDeLactuacio, Etapa etapaAlMomentDeLactuacio, string nivellAlMomentDeLactuacio, int minutsDuradaActuacio, string descripcioActuacio)
        {
            Alumne = alumne;
            TipusActuacio = tipusActuacio;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            CentreAlMomentDeLactuacio = centreAlMomentDeLactuacio;
            EtapaAlMomentDeLactuacio = etapaAlMomentDeLactuacio;
            NivellAlMomentDeLactuacio = nivellAlMomentDeLactuacio;
            MinutsDuradaActuacio = minutsDuradaActuacio;
            DescripcioActuacio = descripcioActuacio;
        }

        public int Id { get; set; }
        public Alumne Alumne { get; set; } = default!;
        public TipusActuacio TipusActuacio { get; set; } = default!;
        public string ObservacionsTipusActuacio { get; set; } = string.Empty;
        public DateTime MomentDeLactuacio { get; set; }
        public CursAcademic CursActuacio { get; set; } = default!;
        public Centre CentreAlMomentDeLactuacio { get; set; } = default!;
        public Etapa EtapaAlMomentDeLactuacio { get; set; } = default!;
        public string NivellAlMomentDeLactuacio { get; set; } = string.Empty;
        public int MinutsDuradaActuacio { get; set; }
        public string DescripcioActuacio { get; set; } = string.Empty;

        // IEtiquetaDescripcio
        public string Etiqueta => $"{MomentDeLactuacio.ToString("dd.MM.yyyy")} - {Alumne.Etiqueta}";
        public string Descripcio => $"{TipusActuacio.Etiqueta}: {ObservacionsTipusActuacio} \n\n{DescripcioActuacio5primeresLinies()}";

        private string DescripcioActuacio5primeresLinies()
        {
            int N = 5;
            var splited = DescripcioActuacio.Split("\n").ToList();
            var n = splited.Count;
            var take = n == N + 1 ? n : N;
            var msg = n > N + 1 ? $"\n( {n - N} línies més ... )" : "";

            splited.AddRange(
                new[] { "", "", "", "", "", "" }
            );

            return string.Join(
                "\n",
                splited.Take(take)
            ) + msg;
        }
    }
}

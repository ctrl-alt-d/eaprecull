using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace DataModels.Models
{
    public class Etapa: IIdEtiquetaDescripcio, IActiu
    {
        public int Id {get; set; }
        public string Codi {get; set; } = string.Empty; // Ex: BAT, ESO
        public string Nom {get; set; }  = string.Empty; // Ex: Batxillerat

        // IActiu
        public bool EsActiu {get; set;}

        // IEtiquetaDescripcio
        public string Etiqueta => Codi;
        public string Descripcio => $"{Nom} {_EsActiuTxt}";
        public string _EsActiuTxt => EsActiu?"":" * Obsolet * ";

        //
        public List<Alumne> Alumnes { get; set; } = new();
    }
}

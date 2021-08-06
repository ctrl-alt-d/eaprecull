using System;
using System.Collections.Generic;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{
    public class Etapa: IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id {get; set; }
        public string Codi {get; set; } = string.Empty; // Ex: BAT, ESO
        public string Nom {get; set; }  = string.Empty; // Ex: Batxillerat

        // IActiu
        public bool EsActiu {get; set;}
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Codi;
        public string Descripcio => $"{Nom} {_EsActiuTxt}";
        public string _EsActiuTxt => EsActiu?"":" * Obsolet * ";

        //
        public List<Alumne> Alumnes { get; set; } = new();
    }
}

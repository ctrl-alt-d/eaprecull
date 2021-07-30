using System;
using System.Collections.Generic;
using CommonInterfaces;


namespace DataModels.Models
{
    public class TipusActuacio: IIdEtiquetaDescripcio, IActiu
    {
        public int Id {get; set; }

        public string Codi {get; set;} = string.Empty; // Ex: Pares
        public string Nom {get; set;} = string.Empty;  // Ex: Entrevista amb Pares

        // IActiu
        public bool EsActiu {get; set;}

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => $"{Codi} {_EsActiuTxt}";
        public string _EsActiuTxt => EsActiu?"":" * Inactiu * ";

        //
        public List<Actuacio> Actuacions { get; set; } = new();
    }
}

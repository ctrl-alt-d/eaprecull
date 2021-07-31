using System;
using System.Collections.Generic;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{
    public class CursAcademic: IIdEtiquetaDescripcio, IModel
    {
        public int Id {get; set; }
        public int AnyInici {get; set;}
        public string Nom {get; set;} = string.Empty;
        public bool EsElCursActual {get; set;}

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => $"{Nom} {_EsElCursActualTxt}";
        private string _EsElCursActualTxt => EsElCursActual?"* Curs Actual *":"";

        //
        public List<Alumne> AlumnesActualitzats { get; set; } = new();
        public List<Actuacio> Actuacions { get; set; } = new();
    }
}

using System;
using CommonInterfaces;

namespace DataModels.Models
{
    public class CursAcademic: INomDescripcio
    {
        public int Id {get; set; }
        public int AnyInici {get; set;}
        public string Nom {get; set;} = string.Empty;
        public string Descripcio => $"{AnyInici}-{AnyInici+1} {_EsElCursActualTxt}";
        private string _EsElCursActualTxt => EsElCursActual?"* Curs Actual *":"";
        public bool EsElCursActual {get; set;}
    }
}

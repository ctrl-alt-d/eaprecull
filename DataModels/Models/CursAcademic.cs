using System;
using System.Collections.Generic;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{
    public class CursAcademic: IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id {get; set; }
        public int AnyInici {get; set;}
        public string Nom {get; set;} = string.Empty;

        // IActiu
        public bool EsActiu {get; set;}
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => $"{Nom} {_EsActiuTxt}";
        private string _EsActiuTxt => EsActiu?"* Curs Actual *":"";

        //
        public List<Alumne> AlumnesActualitzats { get; set; } = new();
        public List<Actuacio> Actuacions { get; set; } = new();
    }
}

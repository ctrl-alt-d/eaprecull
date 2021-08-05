using System;
using System.Collections.Generic;
using CommonInterfaces;
using DataModels.Models.Interfaces;

namespace DataModels.Models
{
    public class Centre: IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id {get; set; }
        public string Codi {get; set;} = string.Empty;
        public string Nom {get; set;} = string.Empty;

        // IActiu
        public bool EsActiu {get; set;}
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => $"{Codi} {_EsActiuTxt}";
        public string _EsActiuTxt => EsActiu?"":" * Inactiu * ";

        //
        public List<Alumne> Alumnes { get; set; } = new();
    }
}

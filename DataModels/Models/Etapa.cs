using System;
using System.Collections.Generic;
using CommonInterfaces;

namespace DataModels.Models
{
    public class Etapa : IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id { get; set; }
        public string Codi { get; set; } = string.Empty; // Ex: BAT, ESO
        public string Nom { get; set; } = string.Empty; // Ex: Batxillerat
        public bool SonEstudisObligatoris { get; set; }

        // IActiu
        public bool EsActiu { get; set; }
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => Codi;

        //
        public List<Alumne> Alumnes { get; set; } = new();
    }
}

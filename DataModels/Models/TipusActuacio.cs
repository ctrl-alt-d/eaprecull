using System;
using System.Collections.Generic;
using CommonInterfaces;


namespace DataModels.Models
{
    public class TipusActuacio : IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id { get; set; }

        public string Codi { get; set; } = string.Empty; // Ex: Pares
        public string Nom { get; set; } = string.Empty;  // Ex: Entrevista amb Pares

        // IActiu
        public bool EsActiu { get; set; }
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => Codi;

        //
        public List<Actuacio> Actuacions { get; set; } = new();
    }
}

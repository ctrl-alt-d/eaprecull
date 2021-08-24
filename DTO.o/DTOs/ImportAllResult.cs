using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class ImportAllResult: IDTOo, IEtiquetaDescripcio
    {


        public int NumAlumnes {get; set; } 
        public int NumActuacions {get; set; } 
        public int NumCentres {get; set; } 
        public int NumEtapes {get; set; } 
        public int NumCursosAcademics {get; set; } 
        public int NumTipusActuacio {get; set; }

        public string Etiqueta => $"{NumActuacions} importades";

        public string Descripcio => $"{NumAlumnes} alumnes, {NumCentres} centres, {NumEtapes} etapes, {NumCursosAcademics} cursos, {NumTipusActuacio} tipus actuació";
    }
}

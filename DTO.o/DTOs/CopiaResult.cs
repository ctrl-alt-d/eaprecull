using System;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    /// <summary>
    /// El que la finestra ensenya quan una còpia ha anat bé. El recompte d'actuacions hi
    /// és a posta: és el que permet a l'usuari veure d'un cop d'ull que la còpia té les
    /// dades que esperava.
    /// </summary>
    public class CopiaResult : IDTOo, IEtiquetaDescripcio
    {
        public CopiaResult(string nom, DateTime quan, long bytes, string desti,
                           int copiesGuardades, int actuacions, string? avis)
        {
            Nom = nom;
            Quan = quan;
            Bytes = bytes;
            Desti = desti;
            CopiesGuardades = copiesGuardades;
            Actuacions = actuacions;
            Avis = avis;
        }

        public string Nom { get; }
        public DateTime Quan { get; }
        public long Bytes { get; }
        public string Desti { get; }
        public int CopiesGuardades { get; }
        public int Actuacions { get; }

        /// <summary>
        /// La còpia és correcta però hi ha hagut una pega que val la pena dir —típicament,
        /// que no s'han pogut retirar les còpies antigues. Null si tot ha anat rodó.
        /// </summary>
        public string? Avis { get; }

        public string Etiqueta => $"Còpia «{Nom}» desada a {Desti}";

        public string Descripcio =>
            $"{Bytes / 1024d / 1024d:N1} MB · {Actuacions:N0} actuacions · se'n guarden {CopiesGuardades}";
    }
}

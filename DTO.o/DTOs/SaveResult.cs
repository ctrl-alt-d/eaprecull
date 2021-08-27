using System;
using System.Collections.Generic;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class SaveResult : IDTOo, IEtiquetaDescripcio
    {
        public SaveResult(string fullPath, string fileName, string folderPath)
        {
            FullPath = fullPath;
            FileName = fileName;
            FolderPath = folderPath;
        }

        public string FullPath { get; }
        public string FileName { get; }
        public string FolderPath { get; }

        public string Etiqueta => FileName;

        public string Descripcio => FullPath;
    }
}

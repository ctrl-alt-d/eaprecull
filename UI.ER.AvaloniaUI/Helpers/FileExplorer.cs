using System;
using System.Diagnostics;
using Dtoo = DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.Helpers
{
    /// <summary>
    /// Obre una carpeta amb el gestor de fitxers del sistema. Abans n'hi havia tres
    /// còpies idèntiques (<c>AlumneRowUserCtrl</c>, <c>UtilitatsWindow</c>,
    /// <c>AlumneInformeViewerWindow</c>).
    /// </summary>
    public static class FileExplorer
    {
        /// <summary>Obre la carpeta del resultat, si n'hi ha cap.</summary>
        public static void Obre(Dtoo.SaveResult? saveResult)
        {
            if (saveResult == null) return;
            Obre(saveResult.FolderPath);
        }

        public static void Obre(string path)
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
            };

            // El verb "open" només l'entén el shell de Windows; a macOS i Linux
            // UseShellExecute ja delega a `open` i `xdg-open` respectivament, i
            // informar-lo hi seria com a mínim inútil.
            if (OperatingSystem.IsWindows())
                psi.Verb = "open";

            Process.Start(psi);
        }
    }
}

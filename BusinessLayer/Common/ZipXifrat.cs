using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Empaqueta fitxers en un zip xifrat amb <strong>AES-256</strong>.
    /// </summary>
    /// <remarks>
    /// <c>System.IO.Compression</c> no sap xifrar, i l'alternativa clàssica —ZipCrypto, la
    /// que obre l'explorador de Windows sense res més— està trencada des dels anys 90.
    /// Aquí hi ha dades de menors, i per això AES-256 encara que calgui 7-Zip, Keka o
    /// WinRAR per obrir el fitxer.
    /// </remarks>
    public static class ZipXifrat
    {
        /// <summary>La mida de clau que fa que SharpZipLib xifri amb AES i no amb ZipCrypto.</summary>
        private const int MidaDeClau = 256;

        /// <summary>Compressió mitjana: la base de dades ja ve compactada del VACUUM.</summary>
        private const int NivellDeCompressio = 6;

        /// <summary>
        /// Escriu <paramref name="cami"/> amb les entrades donades. Les claus del
        /// diccionari són els noms de dins del zip; els valors, els fitxers d'origen.
        /// </summary>
        public static void Escriu(
            string cami,
            string password,
            IReadOnlyDictionary<string, string> entrades)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Un zip de còpia de seguretat sempre va xifrat.", nameof(password));

            using var sortida = new ZipOutputStream(File.Create(cami));

            // UTF-8 als noms d'entrada: hi ha accents i ela geminada als textos del
            // programa, i un nom mal codificat és un fitxer que no es pot extreure.
            sortida.UseZip64 = UseZip64.Dynamic;
            sortida.SetLevel(NivellDeCompressio);
            sortida.Password = password;

            var buffer = new byte[81920];

            foreach (var (nom, origen) in entrades)
            {
                var informacio = new FileInfo(origen);

                var entrada = new ZipEntry(ZipEntry.CleanName(nom))
                {
                    DateTime = informacio.LastWriteTime,
                    Size = informacio.Length,
                    AESKeySize = MidaDeClau,
                    IsUnicodeText = true,
                };

                sortida.PutNextEntry(entrada);

                using (var lectura = File.OpenRead(origen))
                {
                    int llegits;
                    while ((llegits = lectura.Read(buffer, 0, buffer.Length)) > 0)
                        sortida.Write(buffer, 0, llegits);
                }

                sortida.CloseEntry();
            }

            sortida.Finish();
        }

        /// <summary>
        /// Extreu el zip a <paramref name="carpeta"/>. Només el fan servir els tests i el
        /// codi que vulgui comprovar una còpia: el programa no restaura res (§17 del pla).
        /// </summary>
        public static void Extreu(string cami, string password, string carpeta)
        {
            using var zip = new ZipFile(cami) { Password = password };

            Directory.CreateDirectory(carpeta);

            var buffer = new byte[81920];

            foreach (ZipEntry entrada in zip)
            {
                if (!entrada.IsFile)
                    continue;

                var desti = Path.Combine(carpeta, entrada.Name);

                var pare = Path.GetDirectoryName(desti);
                if (!string.IsNullOrEmpty(pare))
                    Directory.CreateDirectory(pare);

                using var lectura = zip.GetInputStream(entrada);
                using var escriptura = File.Create(desti);

                int llegits;
                while ((llegits = lectura.Read(buffer, 0, buffer.Length)) > 0)
                    escriptura.Write(buffer, 0, llegits);
            }
        }

        /// <summary>
        /// Text del <c>LLEGEIX-ME.txt</c> que va dins de cada còpia: sense les
        /// instruccions de restauració, una còpia de seguretat és mitja còpia.
        /// </summary>
        public static string LlegeixMe(string nomDelZip, DateTime quan, int actuacions)
            => new StringBuilder()
                .AppendLine("CÒPIA DE SEGURETAT D'EAP RECULL")
                .AppendLine("===============================")
                .AppendLine()
                .AppendLine($"Fitxer .......... {nomDelZip}")
                .AppendLine($"Feta el ......... {quan:dd/MM/yyyy} a les {quan:HH:mm}")
                .AppendLine($"Actuacions ...... {actuacions:N0}")
                .AppendLine()
                .AppendLine("QUÈ HI HA A DINS")
                .AppendLine("----------------")
                .AppendLine("  BaseDeDades.db   La base de dades sencera.")
                .AppendLine("  Usuari.ini       Les teves dades, si n'hi havia.")
                .AppendLine()
                .AppendLine("COM RESTAURAR-LA")
                .AppendLine("----------------")
                .AppendLine("  1. Tanca EAP Recull.")
                .AppendLine("  2. Obre aquest zip amb 7-Zip, WinRAR o Keka i escriu-hi la contrasenya.")
                .AppendLine("  3. Substitueix el fitxer BaseDeDades.db de la carpeta de dades")
                .AppendLine("     d'EAP Recull pel BaseDeDades.db d'aquest zip.")
                .AppendLine("  4. Torna a obrir EAP Recull.")
                .AppendLine()
                .AppendLine("AVÍS")
                .AppendLine("----")
                .AppendLine("  Aquest zip està xifrat amb AES-256. Si perds la contrasenya no hi ha")
                .AppendLine("  cap manera de recuperar-ne el contingut: ningú no la té desada.")
                .AppendLine("  L'explorador de fitxers de Windows no obre zips amb AES; cal 7-Zip,")
                .AppendLine("  WinRAR, Keka o similar.")
                .ToString();
    }
}

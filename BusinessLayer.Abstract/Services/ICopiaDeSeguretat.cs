using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Generic;
using Dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    /// <summary>
    /// Fa una còpia de seguretat de la base de dades i la porta al destí configurat.
    /// Operació d'un sol ús: <c>using var bl = serveis.GetBLOperation&lt;ICopiaDeSeguretat&gt;()</c>.
    /// </summary>
    /// <remarks>
    /// Tot el que és car —bolcat, verificació, xifratge, retenció— és aquí i és comú; el
    /// destí va darrere d'<see cref="IMagatzemDeCopies"/>, que és l'única peça que canvia
    /// segons on vagin a parar les còpies.
    /// </remarks>
    public interface ICopiaDeSeguretat : IBLOperation
    {
        /// <summary>
        /// Quant es deixa passar abans de proposar una còpia. Constant i no configurable,
        /// pel mateix motiu que les tres còpies que es conserven: ningú ho ha demanat.
        /// </summary>
        const int SetmanesSenseCopia = 2;

        /// <summary>Els destins disponibles en aquesta compilació, per al selector de la finestra.</summary>
        IReadOnlyList<DestiDisponible> Destins { get; }

        /// <summary>Quin hi ha triat i com està configurat.</summary>
        DestiDeCopies DestiActual { get; }

        /// <summary>La clau del destí triat, o cadena buida si no n'hi ha cap.</summary>
        string ClauDelDestiActual { get; }

        /// <summary>Quan es va fer l'última còpia, si se n'ha fet alguna.</summary>
        DateTime? DarreraCopia { get; }

        /// <summary>Quantes còpies es conserven al destí.</summary>
        int CopiesQueEsGuarden { get; }

        /// <summary>
        /// Si toca proposar una còpia en arrencar. Dos requisits alhora: que hagin passat
        /// més de <see cref="SetmanesSenseCopia"/> setmanes i que hi hagi actuacions noves
        /// des de l'última. Mai llança: un problema comptant no ha d'impedir arrencar, i
        /// el pitjor cas és no proposar-la aquesta vegada.
        /// </summary>
        Task<OperationResult<PropostaDeCopia>> CalFerCopia(CancellationToken ct = default);

        /// <param name="password">La que xifra el zip. No es desa enlloc.</param>
        /// <param name="progres">Missatges per a la UI: «Preparant la còpia…», «Desant…».</param>
        Task<OperationResult<Dtoo.CopiaResult>> Run(
            string password, IProgress<string>? progres = null, CancellationToken ct = default);

        /// <summary>Tria i prepara un destí (carpeta triada, o autorització del compte).</summary>
        Task<OperationResult<DestiDeCopies>> ConfiguraDesti(
            string clau, string? parametre, CancellationToken ct = default);

        /// <summary>Les còpies que hi ha al destí, per pintar-les a la finestra.</summary>
        Task<OperationResult<Copies>> Llista(CancellationToken ct = default);

        /// <summary>Oblida el destí configurat. No esborra cap còpia.</summary>
        Task<OperationResult<DestiDeCopies>> Oblida();
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonInterfaces;

namespace BusinessLayer.Abstract.Generic
{
    /// <summary>
    /// La funció de convenció que aparella el que ha canviat amb el que s'ha de refrescar:
    /// <c>Referencies(dto) = { el propi dto } ∪ { tota propietat IIdEtiquetaDescripcio que exposa }</c>.
    /// L'apliquen les dues bandes —l'emissor al DTO que acaba d'escriure, el receptor al que
    /// té pintat— i per això no cal cap mapa de dependències escrit a mà.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Recorre <em>un sol nivell</em>. Endinsar-s'hi tocaria navegacions d'EF: el valor
    /// d'aquestes propietats no és un DTO sinó el model, perquè les projeccions
    /// (<c>DTO.Projections</c>) passen els models sencers al constructor del DTO. Per això
    /// només se'n llegeix l'<c>Id</c> i mai l'<c>Etiqueta</c>, que en algun model és
    /// calculada sobre navegacions.
    /// </para>
    /// <para>
    /// La correspondència tipus → <see cref="Entitat"/> és pel nom, pujant per
    /// <c>BaseType</c>: així val alhora per a <c>DTO.o.DTOs.Alumne</c>, per a
    /// <c>DataModels.Models.Alumne</c> i per a <c>CentreAmbActuacions</c>, que resol pel seu
    /// base <c>Centre</c>. No hi ha proxies de lazy loading configurats
    /// (<c>DataLayer/AppDbContext.cs</c>), o sigui que el tipus en runtime és el real; si
    /// algun dia s'activessin, pujar per <c>BaseType</c> ja ho cobreix.
    /// </para>
    /// </remarks>
    public static class Referencies
    {
        // Això s'executa a cada escriptura i a cada avaluació de fila: la reflexió es
        // cacheja per tipus.
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();
        private static readonly ConcurrentDictionary<Type, Entitat?> CacheEntitats = new();

        /// <summary>Les entitats que un DTO referencia, ell mateix inclòs.</summary>
        public static IReadOnlySet<Referencia> De(object? dto)
        {
            var referencies = new HashSet<Referencia>();
            Acumula(referencies, dto);
            return referencies;
        }

        /// <summary>
        /// La unió de les referències de diversos DTOs. És el cas de <c>BLUpdate</c>, que
        /// publica les del DTO previ i les del nou: moure una actuació d'un alumne a un
        /// altre ha de refrescar els dos comptadors.
        /// </summary>
        public static IReadOnlySet<Referencia> De(params object?[] dtos)
        {
            var referencies = new HashSet<Referencia>();

            foreach (var dto in dtos)
                Acumula(referencies, dto);

            return referencies;
        }

        /// <summary>
        /// Les propietats d'un tipus que porten una altra entitat a dins. Públic perquè és
        /// el que fixa <c>ReferenciesTest</c>: la regla és un escaneig, no una llista.
        /// </summary>
        public static PropertyInfo[] Propietats(Type tipus)
            => Cache.GetOrAdd(tipus, t => t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => typeof(IIdEtiquetaDescripcio).IsAssignableFrom(p.PropertyType))
                .ToArray());

        /// <summary>L'entitat que li correspon a un tipus pel nom, o <c>null</c> si cap.</summary>
        public static Entitat? EntitatDe(Type tipus)
            => CacheEntitats.GetOrAdd(tipus, t =>
            {
                for (var actual = t; actual is not null; actual = actual.BaseType)
                    if (Enum.TryParse<Entitat>(actual.Name, ignoreCase: false, out var entitat))
                        return entitat;

                return null;
            });

        private static void Acumula(HashSet<Referencia> referencies, object? dto)
        {
            if (dto is null)
                return;

            Afegeix(referencies, dto);

            foreach (var propietat in Propietats(dto.GetType()))
                Afegeix(referencies, propietat.GetValue(dto));
        }

        private static void Afegeix(HashSet<Referencia> referencies, object? valor)
        {
            if (valor is not IId ambId)
                return;

            var entitat = EntitatDe(valor.GetType());

            if (entitat is not null)
                referencies.Add(new Referencia(entitat.Value, ambId.Id));
        }
    }
}

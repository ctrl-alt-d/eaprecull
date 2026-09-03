using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.DI
{
    /// <summary>
    /// Registre del BusinessLayer. Per comprensió: cap operació s'enumera a mà.
    /// </summary>
    public static class Injection
    {
        public static IServiceCollection BusinessLayerConfigureServices(this IServiceCollection services)
        {
            // Transient, com abans de R7: cada operació és d'un sol ús i el consumidor
            // la demana per l'IServiceFactory, que és Scoped i li marca el cicle de vida (R1).
            foreach (var (contracte, implementacio) in Operacions())
                services.AddTransient(contracte, implementacio);

            return services;
        }

        /// <summary>
        /// Els parells contracte → implementació del BusinessLayer, per convenció:
        /// cada <c>IXxx</c> de <c>BusinessLayer.Abstract.Services</c> es resol amb la
        /// classe <c>Xxx</c> de <c>BusinessLayer.Services</c>.
        /// </summary>
        /// <remarks>
        /// El filtre és el namespace, no només <see cref="IBLOperation"/>: els contractes
        /// genèrics (<c>ISet&lt;,&gt;</c>, <c>ICreate&lt;&gt;</c>…) també en deriven, viuen
        /// a <c>BusinessLayer.Abstract.Generic</c> i no es registren.
        /// <para>
        /// I la parella es busca pel nom, no per assignabilitat: hi ha herència entre
        /// implementacions —<c>CentreSetAmbActuacions : CentreSet</c>— i per tant més d'una
        /// classe compleix <c>ICentreSet.IsAssignableFrom(…)</c>, cosa que faria ambigu
        /// l'escaneig. El nom desempata; l'assignabilitat es queda com a validació.
        /// </para>
        /// </remarks>
        internal static IEnumerable<(Type Contracte, Type Implementacio)> Operacions()
        {
            var implAsm = typeof(CentreSet).Assembly;
            var implNs = typeof(CentreSet).Namespace;

            var parells = new List<(Type, Type)>();
            var errors = new List<string>();

            foreach (var contracte in Contractes())
            {
                var nom = $"{implNs}.{contracte.Name[1..]}";
                var implementacio = implAsm.GetType(nom);

                if (implementacio is null || implementacio.IsAbstract)
                    errors.Add($"{contracte.Name}: falta la classe {nom}.");
                else if (!contracte.IsAssignableFrom(implementacio))
                    errors.Add($"{contracte.Name}: {nom} no implementa el contracte.");
                else
                    parells.Add((contracte, implementacio));
            }

            // Petar aquí i no silenciar-ho: una interfície nova sense implementació ha de
            // fallar a l'arrencada, no en runtime dins d'un diàleg.
            if (errors.Count > 0)
                throw new InvalidOperationException(
                    "Operacions del BusinessLayer sense implementació per convenció:"
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, errors));

            return parells;
        }

        /// <summary>Les operacions declarades a <c>BusinessLayer.Abstract.Services</c>.</summary>
        internal static IEnumerable<Type> Contractes()
            => typeof(IBLOperation).Assembly.GetTypes()
                .Where(t => t.IsInterface
                         && t.Namespace == typeof(ICentreSet).Namespace
                         && typeof(IBLOperation).IsAssignableFrom(t))
                .OrderBy(t => t.Name);
    }
}

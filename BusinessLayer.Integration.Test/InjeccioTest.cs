using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using BusinessLayer.DI;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// R7 — el registre del BusinessLayer es fa per comprensió. Aquests tests fixen la
    /// convenció (<c>IXxx</c> → <c>Xxx</c>), el cicle de vida i que tot es pugui resoldre.
    /// </summary>
    public class InjeccioTest
    {
        /// <summary>Les operacions declarades a <c>BusinessLayer.Abstract.Services</c>.</summary>
        private static List<Type> Contractes()
            => typeof(IBLOperation).Assembly.GetTypes()
                .Where(t => t.IsInterface
                         && t.Namespace == typeof(ICentreSet).Namespace
                         && typeof(IBLOperation).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

        private static ServiceCollection Serveis()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlite("Data Source=:memory:"));
            services.BusinessLayerConfigureServices();
            return services;
        }

        [Fact]
        public void CadaContracteTeLaSevaImplementacioPerConvencio()
        {
            // El registre és per fàbrica des que el bus de canvis s'injecta per propietat,
            // i per tant el descriptor ja no diu quina classe hi ha darrere: la convenció
            // es comprova sobre el tipus que en surt de debò.
            var serveis = Serveis();
            using var provider = serveis.BuildServiceProvider(validateScopes: true);

            var errors = Contractes()
                .Select(contracte =>
                {
                    var registres = serveis.Where(d => d.ServiceType == contracte).ToList();

                    if (registres.Count != 1)
                        return $"{contracte.Name}: {registres.Count} registres, se n'esperava 1.";

                    if (registres[0].Lifetime != ServiceLifetime.Transient)
                        return $"{contracte.Name}: {registres[0].Lifetime}, s'esperava Transient.";

                    using var operacio = (IBLOperation)provider.GetRequiredService(contracte);

                    if (operacio.GetType().Name != contracte.Name[1..])
                        return $"{contracte.Name}: resolt amb {operacio.GetType().Name}.";

                    return null;
                })
                .OfType<string>()
                .ToList();

            Assert.Empty(errors);
        }

        [Fact]
        public void ElBusDeCanvisEsSingletonIEsRegistraAbansDeLesOperacions()
        {
            // Singleton: hi ha un sol bus per a tota l'aplicació. I abans de les
            // operacions, que és qui l'ha de rebre quan la fàbrica les construeix.
            var serveis = Serveis();

            var descriptor = Assert.Single(serveis, d => d.ServiceType == typeof(INotificadorDeCanvis));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

            var posicioBus = serveis.Select((d, i) => (d, i))
                .First(x => x.d.ServiceType == typeof(INotificadorDeCanvis)).i;

            var primeraOperacio = serveis.Select((d, i) => (d, i))
                .First(x => typeof(IBLOperation).IsAssignableFrom(x.d.ServiceType)).i;

            Assert.True(posicioBus < primeraOperacio,
                "El notificador s'ha de registrar abans de les operacions.");

            using var provider = serveis.BuildServiceProvider(validateScopes: true);
            Assert.Same(
                provider.GetRequiredService<INotificadorDeCanvis>(),
                provider.GetRequiredService<INotificadorDeCanvis>());
        }

        /// <summary>
        /// L'escaneig no ha de registrar res més: ni els contractes genèrics de
        /// <c>BusinessLayer.Abstract.Generic</c> ni les classes base de <c>Common</c>.
        /// </summary>
        [Fact]
        public void NomesEsRegistrenLesOperacionsDelNamespaceServices()
        {
            var registratsPelBL = Serveis()
                .Select(d => d.ServiceType)
                .Where(t => typeof(IBLOperation).IsAssignableFrom(t))
                .ToList();

            Assert.Equal(Contractes().Count, registratsPelBL.Count);
            Assert.Equal(Contractes().OrderBy(t => t.Name), registratsPelBL.OrderBy(t => t.Name));
        }

        [Fact]
        public void TotesLesOperacionsEsResolen()
        {
            using var provider = Serveis().BuildServiceProvider(validateScopes: true);

            var errors = Contractes()
                .Select(contracte =>
                {
                    try
                    {
                        using var operacio = (IBLOperation)provider.GetRequiredService(contracte);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return $"{contracte.Name}: {ex.Message}";
                    }
                })
                .OfType<string>()
                .ToList();

            Assert.Empty(errors);
        }
    }
}

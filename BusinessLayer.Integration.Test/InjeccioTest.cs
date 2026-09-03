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
            var serveis = Serveis();

            var errors = Contractes()
                .Select(contracte =>
                {
                    var registres = serveis.Where(d => d.ServiceType == contracte).ToList();

                    if (registres.Count != 1)
                        return $"{contracte.Name}: {registres.Count} registres, se n'esperava 1.";

                    var registre = registres[0];

                    if (registre.ImplementationType?.Name != contracte.Name[1..])
                        return $"{contracte.Name}: registrat amb {registre.ImplementationType?.Name ?? "res"}.";

                    if (registre.Lifetime != ServiceLifetime.Transient)
                        return $"{contracte.Name}: {registre.Lifetime}, s'esperava Transient.";

                    return null;
                })
                .OfType<string>()
                .ToList();

            Assert.Empty(errors);
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

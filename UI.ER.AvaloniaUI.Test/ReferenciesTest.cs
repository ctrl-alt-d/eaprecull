using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using Xunit;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// El bus de canvis no té cap mapa de dependències escrit a mà: emissor i receptor
    /// apliquen la mateixa funció de convenció. Aquests tests la fixen, i ho fan per
    /// escaneig — un DTO nou amb una referència nova hi entra sol.
    /// </summary>
    public class ReferenciesTest
    {
        private static Type[] DtosDeSortida { get; } =
            typeof(Dtoo.Actuacio).Assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }
                         && t.Namespace == typeof(Dtoo.Actuacio).Namespace)
                .OrderBy(t => t.Name)
                .ToArray();

        [Fact]
        public void CadaDtoDeSortidaExposaTotesLesSevesReferencies()
        {
            // Referencies.Propietats ha de trobar exactament les propietats que porten
            // una altra entitat a dins. Ni una de menys —la fila no es refrescaria— ni
            // una de més.
            var errors = DtosDeSortida
                .Select(dto =>
                {
                    var esperades = dto
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => typeof(IIdEtiquetaDescripcio).IsAssignableFrom(p.PropertyType))
                        .Select(p => p.Name)
                        .OrderBy(n => n);

                    var trobades = Referencies.Propietats(dto).Select(p => p.Name).OrderBy(n => n);

                    return esperades.SequenceEqual(trobades)
                        ? null
                        : $"{dto.Name}: esperades [{string.Join(", ", esperades)}], "
                          + $"trobades [{string.Join(", ", trobades)}]";
                })
                .OfType<string>()
                .ToList();

            Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
        }

        [Fact]
        public void UnaActuacioReferenciaTotElQuePinta()
        {
            // El cas que justifica tot el mecanisme: una actuació porta a dins l'alumne,
            // el tipus, el curs, el centre i l'etapa, i qualsevol d'ells la fa obsoleta.
            var actuacio = ActuacioDeProva();

            Assert.Equal(
                new HashSet<Referencia>
                {
                    new(Entitat.Actuacio, 1),
                    new(Entitat.Alumne, 7),
                    new(Entitat.TipusActuacio, 4),
                    new(Entitat.CursAcademic, 2),
                    new(Entitat.Centre, 3),
                    new(Entitat.Etapa, 5),
                },
                Referencies.De(actuacio));
        }

        [Fact]
        public void LaSobrecarregaUneixLesReferenciesDeDosDtos()
        {
            // És el cas de BLUpdate: moure una actuació de l'alumne #7 al #9 ha de
            // refrescar els comptadors dels dos.
            var previa = ActuacioDeProva(alumneId: 7);
            var nova = ActuacioDeProva(alumneId: 9);

            var unio = Referencies.De(previa, nova);

            Assert.Contains(new Referencia(Entitat.Alumne, 7), unio);
            Assert.Contains(new Referencia(Entitat.Alumne, 9), unio);
        }

        [Fact]
        public void UnDtoQueNoEsCapEntitatNoReferenciaRes()
        {
            // El resultat d'una operació massiva no és cap entitat: per això es publica
            // com a CanviDeDomini.Tot i no per referències.
            Assert.Empty(Referencies.De(new Dtoo.EtiquetaDescripcio("etiqueta", "descripció")));
            Assert.Empty(Referencies.De((object?)null));
        }

        [Fact]
        public void UnCentreAmbActuacionsResolPelSeuBase()
        {
            // La correspondència és pel nom pujant per BaseType: CentreAmbActuacions no
            // és cap valor de l'enum, però el seu base Centre sí.
            Assert.Equal(Entitat.Centre, Referencies.EntitatDe(typeof(Dtoo.CentreAmbActuacions)));
        }

        [Fact]
        public void LaConvencioValTambePelsModelsDEf()
        {
            // Les propietats d'un DTO de sortida no porten un altre DTO sinó el model:
            // les projeccions hi passen els models sencers. Les dues bandes han de
            // resoldre a la mateixa entitat.
            Assert.Equal(Entitat.Alumne, Referencies.EntitatDe(typeof(Models.Alumne)));
            Assert.Equal(Entitat.Alumne, Referencies.EntitatDe(typeof(Dtoo.Alumne)));
        }

        private static Dtoo.Actuacio ActuacioDeProva(int alumneId = 7)
        {
            var curs = new Dtoo.CursAcademic(2, 2024, "2024-25", true, "2024-25", "", 0);
            var centre = new Dtoo.Centre(3, "C", "Centre", true, "Centre", "");
            var etapa = new Dtoo.Etapa(5, "ESO", "ESO", true, true, "ESO", "");
            var tipus = new Dtoo.TipusActuacio(4, "T", "Tipus", true, "Tipus", "");
            var alumne = new Dtoo.Alumne(
                alumneId, "Nom", "Cognoms", null, centre, curs, etapa, "", null, "", null, "",
                true, "Cognoms, Nom", "", "", 0);

            return new Dtoo.Actuacio(
                1, alumne, tipus, "", DateTime.Today, curs, centre, etapa, "", 30, "", "", "");
        }
    }
}

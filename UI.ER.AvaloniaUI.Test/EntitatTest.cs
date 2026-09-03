using System;
using System.Linq;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using Xunit;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// L'enum <see cref="Entitat"/> és l'única llista escrita a mà de tot el bus, i per
    /// això té un test que la lliga a les dues bandes que hi han de casar pel nom: els
    /// DTOs de sortida i els models d'EF. Fallada ràpida, com la d'arrencada de les vistes.
    /// </summary>
    public class EntitatTest
    {
        private static readonly Entitat[] Entitats = Enum.GetValues<Entitat>();

        [Fact]
        public void CadaEntitatTeElSeuDtoIElSeuModel()
        {
            var errors = Entitats
                .Select(entitat =>
                {
                    var dto = typeof(Dtoo.Actuacio).Assembly
                        .GetType($"{typeof(Dtoo.Actuacio).Namespace}.{entitat}");
                    var model = typeof(Models.Actuacio).Assembly
                        .GetType($"{typeof(Models.Actuacio).Namespace}.{entitat}");

                    if (dto is null) return $"{entitat}: no hi ha cap DTO amb aquest nom.";
                    if (model is null) return $"{entitat}: no hi ha cap model amb aquest nom.";

                    if (Referencies.EntitatDe(dto) != entitat) return $"{entitat}: el DTO no hi resol.";
                    if (Referencies.EntitatDe(model) != entitat) return $"{entitat}: el model no hi resol.";

                    return null;
                })
                .OfType<string>()
                .ToList();

            Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
        }

        [Fact]
        public void CapEntitatDelDominiEsQuedaForaDeLEnum()
        {
            // L'altre sentit: una entitat nova amb el seu DTO i el seu model, però sense
            // valor a l'enum, no seria mai afectada per cap canvi i les seves llistes no
            // es refrescarien mai. Aquí es veu de seguida.
            var fora =
                (from tipus in typeof(Models.Actuacio).Assembly.GetTypes()
                 where tipus is { IsAbstract: false, IsGenericTypeDefinition: false }
                    && tipus.Namespace == typeof(Models.Actuacio).Namespace
                    && typeof(IIdEtiquetaDescripcio).IsAssignableFrom(tipus)
                    && Referencies.EntitatDe(tipus) is null
                 select tipus.Name)
                .ToList();

            Assert.True(fora.Count == 0,
                "Models del domini sense valor a l'enum Entitat: " + string.Join(", ", fora));
        }
    }
}

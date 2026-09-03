using System;
using System.Linq;
using System.Reflection;
using UI.ER.AvaloniaUI.Services;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R0 — les vistes tenen dos constructors: el de DI i el pont sense paràmetres.
    /// Aquests tests protegeixen les dues meitats del patró.
    /// </summary>
    public class ConstructorsDeVistaTest
    {
        [Fact]
        public void TotaVistaTeConstructorPublicSenseParametres()
        {
            // Els *RowUserCtrl els instancia el ListBox.ItemTemplate des de l'AXAML.
            // A les finestres, sense constructor buit el compilador d'Avalonia emet
            // AVLN3001 («won't be reachable via runtime loader»).
            var sense = Vistes.Totes
                .Where(v => v.GetConstructor(Type.EmptyTypes) is null)
                .Select(v => v.Name)
                .ToList();

            Assert.True(sense.Count == 0,
                "Vistes sense constructor públic sense paràmetres: " + string.Join(", ", sense));
        }

        [Fact]
        public void ElConstructorPontEncadenaAmbElDeDI()
        {
            // Una vista que necessita la factory té dos constructors. Si el buit no
            // encadena amb el de DI (: this(App.Services.GetRequiredService<...>())),
            // el camp queda null i peta en obrir el primer diàleg — mai en arrencar.
            var trencades = Vistes.Totes
                .Select(v => new
                {
                    Vista = v,
                    Buit = v.GetConstructor(Type.EmptyTypes),
                    AmbFactory = v.GetConstructor([typeof(IWindowFactory)]),
                })
                .Where(x => x.AmbFactory is not null)
                .Where(x => x.Buit is null || !Crida(x.Buit, x.AmbFactory!))
                .Select(x => x.Vista.Name)
                .ToList();

            Assert.True(trencades.Count == 0,
                "Vistes on el constructor sense paràmetres no encadena amb el de "
                + "IWindowFactory: " + string.Join(", ", trencades));
        }

        [Fact]
        public void LesVistesQueObrenDialegsDemanenLaFactory()
        {
            // Contrapartida del test anterior: si una vista guarda un IWindowFactory,
            // ha de tenir per força el constructor que el rep.
            var problemes = Vistes.Totes
                .Where(Vistes.UsaLaFactory)
                .Where(v => v.GetConstructor([typeof(IWindowFactory)]) is null)
                .Select(v => v.Name)
                .ToList();

            Assert.True(problemes.Count == 0,
                "Vistes amb un camp IWindowFactory però sense constructor que el rebi: "
                + string.Join(", ", problemes));
        }

        /// <summary>
        /// Cerca al cos d'<paramref name="origen"/> una instrucció <c>call</c> (0x28) amb
        /// el token de metadades de <paramref name="desti"/>. És com el compilador tradueix
        /// <c>: this(...)</c>. L'escaneig és per força bruta sobre l'IL, però els cossos
        /// d'aquests constructors són de poques instruccions.
        /// </summary>
        private static bool Crida(ConstructorInfo origen, ConstructorInfo desti)
        {
            var il = origen.GetMethodBody()?.GetILAsByteArray();
            if (il is null) return false;

            const byte OpCodeCall = 0x28;
            var token = desti.MetadataToken;

            for (var i = 0; i + 4 < il.Length; i++)
                if (il[i] == OpCodeCall && BitConverter.ToInt32(il, i + 1) == token)
                    return true;

            return false;
        }
    }
}

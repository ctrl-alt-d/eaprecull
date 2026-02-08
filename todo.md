# TODO - Millores d'Inversió de Dependències i SOLID

> Cada secció conté un prompt autònom per executar en un context net de IA.
> Ordre recomanat d'execució: P4 → P5 → P7 → P1 → P6 → P3+P2 (els dos últims junts).

---

## P4. Netejar `using` innecessaris als conversors

**Impacte:** Baix  
**Fitxers:** `UI.ER.ViewModels/Services/StringIntConverter.cs`, `UI.ER.ViewModels/Services/StringDateConverter.cs`

### Prompt

```
Obre els fitxers `UI.ER.ViewModels/Services/StringIntConverter.cs` i `UI.ER.ViewModels/Services/StringDateConverter.cs`.

Ambdós tenen `using` innecessaris que no s'utilitzen al codi:
- `using Microsoft.Extensions.DependencyInjection;`
- `using BusinessLayer.DI;`
- `using BusinessLayer.Abstract.Generic;`
- `using DataLayer.DI;`

Elimina'ls. Deixa només els `using` que realment es fan servir.
```

---

## P5. Eliminar la interfície morta `IActuacioDelete`

**Impacte:** Baix  
**Fitxers:** `BusinessLayer.Abstract/Services/IActuacioDelete.cs`

### Prompt

```
A `BusinessLayer.Abstract/Services/IActuacioDelete.cs` hi ha una interfície `IActuacioDelete` que no té cap implementació ni cap registre al contenidor DI. Cap altre fitxer la referencia.

Comprova que no hi ha cap ús de `IActuacioDelete` a tot el projecte (cerca al codi). Si és codi mort, elimina el fitxer.
```

---

## P7. Canviar `AppDbContextFactory` a `protected`

**Impacte:** Baix  
**Fitxer:** `BusinessLayer/Common/BLOperation.cs`

### Prompt

```
A `BusinessLayer/Common/BLOperation.cs`, la propietat `AppDbContextFactory` és `public readonly`:

```csharp
public readonly IDbContextFactory<AppDbContext> AppDbContextFactory;
```

Hauria de ser `protected` per respectar l'encapsulament. Cap codi extern a la jerarquia de `BLOperation` necessita accedir-hi.

Cerca tots els usos de `AppDbContextFactory` al projecte. Si només es fa servir dins de `BLOperation` i les seves subclasses, canvia'l a `protected readonly`. Si trobés algun ús extern, informa-me'n abans de fer canvis.
```

---

## P1. Fer que `BLOperation` implementi `IBLOperation`

**Impacte:** Mig  
**Fitxer:** `BusinessLayer/Common/BLOperation.cs`

### Prompt

```
A `BusinessLayer/Common/BLOperation.cs`, la classe base `BLOperation` declara `: IDisposable` però no implementa `IBLOperation` (que es troba a `BusinessLayer.Abstract.Generic`). 

`IBLOperation` estén `IDisposable`, per tant el canvi és compatible. Ara funciona per accident perquè les subclasses com `BLSet` implementen `ISet<,>` que sí hereda de `IBLOperation`. Però si algú creés un servei directe de `BLOperation`, no seria resolvible com `IBLOperation`.

Canvia la declaració de:
```csharp
public abstract class BLOperation : IDisposable
```
a:
```csharp
public abstract class BLOperation : IBLOperation
```

Afegeix el `using BusinessLayer.Abstract.Generic;` si no hi és. Com que `IBLOperation : IDisposable`, no cal mantenir `: IDisposable` explícitament.

Comprova que compila correctament.
```

---

## P6. Corregir `Dispose` absent en 4 crides a `GetBLOperation`

**Impacte:** Mig  
**Fitxers:** `UI.ER.ViewModels/ViewModels/ActuacioSetViewModel.cs`, `UI.ER.ViewModels/ViewModels/AlumneSetViewModel.cs`

### Prompt

```
Hi ha 4 llocs on es crida `SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(...)` sense `using`, cosa que no disposa el `DbContext` intern del servei BL. Això és un memory leak potencial.

Els 4 llocs són:

1. `UI.ER.ViewModels/ViewModels/ActuacioSetViewModel.cs` — cerca:
   ```csharp
   var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new Dtoi.EsActiuParms(true));
   ```
   (apareix 2 cops al fitxer)

2. `UI.ER.ViewModels/ViewModels/AlumneSetViewModel.cs` — cerca:
   ```csharp
   var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new Dtoi.EsActiuParms(true));
   ```
   (apareix 2 cops al fitxer)

En cada cas, canvia el patró a:
```csharp
using var blCurs = SuperContext.GetBLOperation<ICursAcademicSet>();
var cursActual_dto = await blCurs.FromPredicate(new Dtoi.EsActiuParms(true));
```

Assegura't que la variable `blCurs` (o similar) no col·lisioni amb altres variables del mateix scope. Mira el context de cada ús per escollir un nom adequat.

Comprova que compila correctament.
```

---

## P3 + P2. Moure el Composition Root i crear `IServiceFactory`

**Impacte:** Alt  
**Fitxers principals:** `UI.ER.ViewModels/Services/SuperContext.cs`, `UI.ER.ViewModels/UI.ER.ViewModels.csproj`, `UI.ER.AvaloniaUI/UI.ER.AvaloniaUI.csproj`, `UI.ER.AvaloniaUI/App.axaml.cs` + tots els ViewModels que usen `SuperContext`

> Aquests dos canvis van junts perquè són interdependents.

### Context del problema

Actualment `SuperContext` és una classe estàtica a `UI.ER.ViewModels/Services/SuperContext.cs` que:
1. Crea un `ServiceProvider` singleton internament.
2. Referencia directament `BusinessLayer.DI.Injection` i `DataLayer.DI.Injection` per registrar serveis.
3. Tots els ViewModels la criden directament: `SuperContext.GetBLOperation<IAlumneSet>()`.

Això causa dues violacions de DIP:
- `UI.ER.ViewModels` referencia `BusinessLayer.csproj` (implementació concreta) quan només hauria de dependre de `BusinessLayer.Abstract`.
- `SuperContext` és un Service Locator estàtic (anti-patró), no testejable ni substituïble.

### Prompt

```
Necessito reestructurar la inversió de dependències del projecte. El context del projecte està descrit al fitxer `agents.md` a l'arrel.

#### Situació actual

`UI.ER.ViewModels/Services/SuperContext.cs` és una classe estàtica que actua com a Service Locator:

```csharp
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;

namespace UI.ER.AvaloniaUI.Services
{
    public static class SuperContext
    {
        private static ServiceProvider? _ServiceProvider;
        private static ServiceProvider GetServiceProvider()
        {
            _ServiceProvider = _ServiceProvider ??
                new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .BuildServiceProvider();
            return _ServiceProvider;
        }

        public static T GetBLOperation<T>() where T : IBLOperation
        {
            return GetServiceProvider().GetRequiredService<T>();
        }
    }
}
```

Tots els ViewModels la criden així:
```csharp
using var bl = SuperContext.GetBLOperation<ICentreSet>();
```

`UI.ER.ViewModels.csproj` referencia `BusinessLayer.csproj` (concret) i `BusinessLayer.Abstract.csproj`.

#### Canvis a fer

**Pas 1: Crear la interfície `IServiceFactory`**

Crea `BusinessLayer.Abstract/Generic/IServiceFactory.cs`:
```csharp
namespace BusinessLayer.Abstract.Generic
{
    public interface IServiceFactory
    {
        T GetBLOperation<T>() where T : IBLOperation;
    }
}
```

**Pas 2: Transformar `SuperContext`**

Canvia `SuperContext` a `UI.ER.ViewModels/Services/SuperContext.cs` perquè sigui una classe no estàtica que implementi `IServiceFactory`, PERÒ que exposi també una API estàtica compatible (per no haver de canviar tots els ViewModels d'un cop, que reben serveis via Service Locator, no via constructor injection):

```csharp
using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace UI.ER.ViewModels.Services
{
    public class SuperContext : IServiceFactory
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetBLOperation<T>() where T : IBLOperation
        {
            return GetProvider().GetRequiredService<T>();
        }

        public static T Resolve<T>() where T : IBLOperation
        {
            return GetProvider().GetRequiredService<T>();
        }

        private static IServiceProvider GetProvider()
        {
            return _serviceProvider
                ?? throw new InvalidOperationException("SuperContext no ha estat inicialitzat. Crida SuperContext.Initialize() al Composition Root.");
        }
    }
}
```

Canvia el namespace de `UI.ER.AvaloniaUI.Services` a `UI.ER.ViewModels.Services`.

**Pas 3: Actualitzar tots els ViewModels**

Cerca tots els `SuperContext.GetBLOperation<` i canvia'ls per `SuperContext.Resolve<`. Fes un find-and-replace. Actualitza també els `using` si el namespace ha canviat (de `UI.ER.AvaloniaUI.Services` a `UI.ER.ViewModels.Services`).

**Pas 4: Crear el Composition Root**

A `UI.ER.AvaloniaUI/App.axaml.cs` (o on s'inicialitza l'aplicació), afegeix la configuració DI:

```csharp
var services = new ServiceCollection()
    .DataLayerConfigureServices()
    .BusinessLayerConfigureServices();

services.AddSingleton<IServiceFactory, SuperContext>();

var provider = services.BuildServiceProvider();
SuperContext.Initialize(provider);
```

Assegura't que els `using` necessaris estiguin presents (`BusinessLayer.DI`, `DataLayer.DI`, `UI.ER.ViewModels.Services`, `BusinessLayer.Abstract.Generic`).

**Pas 5: Actualitzar les referències de projectes**

Al `UI.ER.ViewModels.csproj`:
- ELIMINA la referència a `BusinessLayer.csproj`
- MANTÉ la referència a `BusinessLayer.Abstract.csproj`

Al `UI.ER.AvaloniaUI.csproj`:
- AFEGEIX referència a `BusinessLayer.csproj` (si no la té ja transitiva)
- AFEGEIX referència a `DataLayer.csproj`

**Pas 6: Eliminar `using` innecessaris**

Dels fitxers `StringIntConverter.cs` i `StringDateConverter.cs` a `UI.ER.ViewModels/Services/`, elimina els `using BusinessLayer.DI`, `using DataLayer.DI` i `using BusinessLayer.Abstract.Generic` si ja no es fan servir.

**Verificació final:**
- Comprova que `UI.ER.ViewModels` NO té cap `using BusinessLayer.Services`, `using BusinessLayer.DI` ni `using DataLayer` a cap fitxer.
- Compila tot el projecte i assegura't que no hi ha errors.
```

---

## Checklist de verificació final

Després d'aplicar tots els canvis, verifica:

- [ ] P4: Els conversors no tenen `using` innecessaris
- [ ] P5: `IActuacioDelete.cs` eliminat (si era codi mort)
- [ ] P7: `AppDbContextFactory` és `protected readonly`
- [ ] P1: `BLOperation` implementa `IBLOperation`
- [ ] P6: Tots els `GetBLOperation` tenen `using var`
- [ ] P2+P3: `UI.ER.ViewModels` no referencia `BusinessLayer.csproj` ni `DataLayer`
- [ ] P2+P3: El Composition Root està a `UI.ER.AvaloniaUI`
- [ ] El projecte compila sense errors
- [ ] L'aplicació arrenca i funciona correctament

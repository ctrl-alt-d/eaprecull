# EapRecull - Guia de Context per a Agents IA

> Document generat per proporcionar tot el context necessari per entendre, mantenir i ampliar aquesta aplicació.

> **Comença per [`ARQUITECTURA.md`](ARQUITECTURA.md)**: hi ha el mapa global —capes,
> dependències, fluxos, invariants i deutes coneguts—. Aquest document és la recepta
> pas a pas; aquell és el terreny.

## 1. Visió General de l'Arquitectura

### Tipus d'Arquitectura

Aquesta solució utilitza una **arquitectura per capes (Layered Architecture)** amb separació clara de responsabilitats, seguint principis de **Clean Architecture** i patrons **Domain-Driven Design (DDD) lleuger**.

```
┌─────────────────────────────────────────────────────────────────┐
│                    UI (Avalonia UI)                             │
│         UI.ER.AvaloniaUI + UI.ER.ViewModels                     │
├─────────────────────────────────────────────────────────────────┤
│               Capa de Serveis / Business Layer                  │
│         BusinessLayer + BusinessLayer.Abstract                  │
├─────────────────────────────────────────────────────────────────┤
│                   DTOs (Transferència)                          │
│           DTO.i (Input) + DTO.o (Output) + DTO.Projections      │
├─────────────────────────────────────────────────────────────────┤
│                    Capa de Dades                                │
│         DataLayer + DataModels + DataModels.Configuration       │
├─────────────────────────────────────────────────────────────────┤
│                  Interfícies Comunes                            │
│                    CommonInterfaces                             │
└─────────────────────────────────────────────────────────────────┘
```

### Responsabilitats de Cada Projecte

| Projecte | Responsabilitat |
|----------|-----------------|
| **CommonInterfaces** | Interfícies bàsiques compartides (`IId`, `IEtiquetaDescripcio`, `IActiu`, `IActivable`) |
| **DataModels** | Entitats del domini (models de dades persistits) |
| **DataModels.Configuration** | Configuració EF Core de les entitats (Fluent API) |
| **DataLayer** | Context de base de dades, migracions, configuració de connexió |
| **DTO.i** | DTOs d'entrada (paràmetres de les operacions) |
| **DTO.o** | DTOs de sortida (resultats de les operacions) |
| **DTO.Projections** | Expressions de projecció Model → DTO |
| **BusinessLayer.Abstract** | Interfícies dels serveis i tipus de resultat (`OperationResult`, `BrokenRule`) |
| **BusinessLayer** | Implementació de la lògica de negoci |
| **UI.ER.ViewModels** | ViewModels per MVVM amb ReactiveUI |
| **UI.ER.AvaloniaUI** | Vistes AXAML, code-behind, helpers i controls personalitzats |

### Patrons Arquitectònics Principals

1. **MVVM (Model-View-ViewModel)** - Capa de presentació amb ReactiveUI
2. **Repository Pattern implícit** - Via `DbSet<T>` de EF Core
3. **Generic Service Pattern** - Classes base genèriques (`BLCreate<TModel, TParm, TDTOo>`)
4. **Projection Pattern** - Expressions `ToDto` per transformar entitats a DTOs
5. **Rule Checker Pattern** - Sistema de validació amb `RuleChecker<T>`
6. **Factory Pattern** - `IDbContextFactory<AppDbContext>` per crear contextos
7. **Dependency Injection** - Configuració centralitzada a `BusinessLayer.DI.Injection`

### Classes Base de Serveis

| Classe Base | Propòsit | Mètodes Principals |
|-------------|----------|-------------------|
| `BLOperation` | Base per a tots els serveis | `GetContext()`, `Perfection<T>()`, `LoadReference()` |
| `BLSet<TModel, TParm, TDTOo>` | Consultes/llistats | `FromPredicate()`, `FromId()`, `CountFromPredicate()` |
| `BLCreate<TModel, TParm, TDTOo>` | Creació d'entitats | `Create()`, `PreInitialize()`, `InitializeModel()`, `PostAdd()` |
| `BLUpdate<TModel, TParm, TDTOo>` | Actualització d'entitats | `Update()`, `PreUpdate()`, `UpdateModel()`, `PostUpdate()` |
| `BLActivaDesactiva<TModel, TDTOo>` | Soft-delete toggle | `Activa()`, `Desactiva()`, `Toggle()` |
| `BLReport<TResult>` | Generació d'informes/fitxers | `ExecuteReport()`, `CalculatePath()`, `GetTemplatesPath()` |
| `BLBatchOperation<TResult>` | Operacions massives | `ExecuteBatch()` |

### Flux de Dependències

```
CommonInterfaces ← DataModels ← DataModels.Configuration ← DataLayer
                            ↑
DTO.i ← DTO.o ← DTO.Projections
    ↑
BusinessLayer.Abstract ← BusinessLayer
                              ↑
UI.ER.ViewModels ← UI.ER.AvaloniaUI
```

---

## 2. Models i Estructura del Domini

### Localització dels Models

| Tipus de Model | Ubicació | Namespace |
|----------------|----------|-----------|
| **Entitats (Models de domini)** | `DataModels/Models/` | `DataModels.Models` |
| **DTOs d'entrada (Parms)** | `DTO.i/DTOs/` | `DTO.i.DTOs` |
| **DTOs de sortida** | `DTO.o/DTOs/` | `DTO.o.DTOs` |
| **Projeccions** | `DTO.Projections/` | `DTO.Projections` |
| **ViewModels** | `UI.ER.ViewModels/ViewModels/` | `UI.ER.ViewModels.ViewModels` |

### Entitats del Domini Actuals

- `Alumne` - Alumne amb dades personals i educatives
- `Actuacio` - Registre d'una actuació sobre un alumne
- `Centre` - Centre educatiu
- `CursAcademic` - Curs acadèmic
- `Etapa` - Etapa educativa (ESO, BAT, etc.)
- `TipusActuacio` - Tipus d'actuació

### Interfícies que Implementen les Entitats

Totes les entitats implementen `IModel` (marcador). A més:

```csharp
// Exemple d'una entitat típica
public class Centre : IIdEtiquetaDescripcio, IActivable, IModel
{
    public int Id { get; set; }                    // De IId
    public string Etiqueta => Nom;                 // De IEtiquetaDescripcio  
    public string Descripcio => Codi;              // De IEtiquetaDescripcio
    public bool EsActiu { get; set; }              // De IActiu
    public void SetActiu() => EsActiu = true;      // De IActivable
    public void SetInactiu() => EsActiu = false;   // De IActivable
}
```

### Interfícies Disponibles

| Interfície | Propòsit |
|------------|----------|
| `IId` | Proporciona `int Id { get; }` |
| `IEtiquetaDescripcio` | Proporciona `Etiqueta` i `Descripcio` per mostrar a UI |
| `IActiu` | Proporciona `bool EsActiu { get; }` |
| `IActivable` | Estén `IActiu` amb `SetActiu()` i `SetInactiu()` |
| `IIdEtiquetaDescripcio` | Combina `IId` + `IEtiquetaDescripcio` |
| `IModel` | Marcador per a entitats persistides (a `CommonInterfaces`) |
| `IDtoi` | Marcador per a DTOs d'entrada |
| `IDTOo` | Marcador per a DTOs de sortida |

### Convencions de Noms

**DTOs d'entrada (`DTO.i`):**
- `{Entitat}CreateParms` - Per crear noves entitats
- `{Entitat}UpdateParms` - Per actualitzar entitats (inclou `IId`)
- `{Entitat}SearchParms` - Per cercar/filtrar entitats
- `ActivaDesactivaParms` - Paràmetre per operacions d'activació/desactivació
- `EsActiuParms` - Paràmetre genèric per filtrar per `EsActiu`
- `EmptyParms` - Quan no cal cap paràmetre
- `EmptyPaginatedParms` - Quan no cal cap paràmetre però es vol paginació

**DTOs de sortida (`DTO.o`):**
- Mateix nom que l'entitat: `Alumne`, `Centre`, `Actuacio`, `CursAcademic`, `Etapa`, `TipusActuacio`
- DTOs addicionals: `CentreAmbActuacions`, `EtiquetaDescripcio`, `SaveResult`, `ImportAllResult`, `AlumneInformeViewerData`

**ViewModels:**
- `{Entitat}SetViewModel` - Llista/cerca d'entitats
- `{Entitat}CreateViewModel` - Formulari de creació
- `{Entitat}UpdateViewModel` - Formulari d'edició
- `{Entitat}RowViewModel` - Representació d'una fila a la llista

---

## 3. Persistència i Accés a Dades

### Context de Base de Dades

El context es defineix a `DataLayer/AppDbContext.cs`:

```csharp
public class AppDbContext : DbContext
{
    public virtual DbSet<Actuacio> Actuacions => Set<Actuacio>();
    public virtual DbSet<Alumne> Alumnes => Set<Alumne>();
    public virtual DbSet<Centre> Centres => Set<Centre>();
    public virtual DbSet<CursAcademic> CursosAcademics => Set<CursAcademic>();
    public virtual DbSet<Etapa> Etapes => Set<Etapa>();
    public virtual DbSet<TipusActuacio> TipusActuacions => Set<TipusActuacio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationAssembly).Assembly);
    }
}
```

### Configuració de les Entitats

Les configuracions Fluent API es troben a `DataModels.Configuration/Configurations/`:

```csharp
// Exemple: DataModels.Configuration/Configurations/Alumne.cs
public class Alumne : IEntityTypeConfiguration<DM.Alumne>
{
    public void Configure(EntityTypeBuilder<DM.Alumne> builder)
    {
        builder.HasOne(m => m.CursDarreraActualitacioDades!)
               .WithMany(r => r.AlumnesActualitzats);

        builder.HasOne(m => m.EtapaActual!)
               .WithMany(r => r.Alumnes);

        builder.HasOne(m => m.CentreActual!)
               .WithMany(r => r.Alumnes);
    }
}
```

### Patró d'Accés a Dades

**No s'utilitzen repositoris explícits.** L'accés es fa directament via:

1. `IDbContextFactory<AppDbContext>` - Injectat als serveis
2. `GetContext().Set<TModel>()` - Per accedir a les col·leccions
3. `Perfection<TModel>(id)` - Mètode helper per carregar entitats per ID

### Base de Dades

- **Tipus:** SQLite
- **Ubicació:** `EapRecullData/BaseDeDades.db` (a Documents en mode DEBUG)
- **Migracions:** S'apliquen automàticament a l'arrencada via `Database.Migrate()`, un cop
  construït el contenidor (`MigraBaseDeDades()`). Hi ha **una sola migració**,
  `20210821093532_inicial`, present a totes les instal·lacions existents.

> ⚠️ **Aplicar-les és automàtic; generar-les, no.** El warning
> `PendingModelChangesWarning` està silenciat a `AppOptionsBuilderConf`: si canvies una
> entitat i no generes migració, **l'aplicació arrenca sense dir res** i peta en runtime a
> la primera consulta que toqui la columna que falta. A més, `.config/dotnet-tools.json`
> té `dotnet-ef` fixat a `6.0.6` amb el projecte en EF Core 10: cal pujar-lo primer.
>
> El procediment complet i el perquè són a [`ARQUITECTURA.md` §6](ARQUITECTURA.md#6-persistència).

---

## 4. Guia per Crear un CRUD Complet

### Pas 1: Crear l'Entitat

**Ubicació:** `DataModels/Models/{Entitat}.cs`

```csharp
using CommonInterfaces;

namespace DataModels.Models
{
    public class NovaEntitat : IIdEtiquetaDescripcio, IActivable, IModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Codi { get; set; } = string.Empty;
        
        // IActiu
        public bool EsActiu { get; set; }
        public void SetActiu() => EsActiu = true;
        public void SetInactiu() => EsActiu = false;

        // IEtiquetaDescripcio
        public string Etiqueta => Nom;
        public string Descripcio => Codi;

        // Relacions (si escau)
        public List<AltraEntitat> Relacions { get; set; } = new();
    }
}
```

### Pas 2: Configurar l'Entitat (si té relacions)

**Ubicació:** `DataModels.Configuration/Configurations/{Entitat}.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class NovaEntitat : IEntityTypeConfiguration<DM.NovaEntitat>
    {
        public void Configure(EntityTypeBuilder<DM.NovaEntitat> builder)
        {
            builder.HasOne(m => m.Relacio!)
                   .WithMany(r => r.NovaEntitats);
        }
    }
}
```

### Pas 3: Registrar al DbContext i generar la migració

**Ubicació:** `DataLayer/AppDbContext.cs`

```csharp
public virtual DbSet<NovaEntitat> NovesEntitats => Set<NovaEntitat>();
```

**I tot seguit, la migració.** Aquest pas no és opcional i **res no t'avisarà si te'l
saltes**: el warning de canvis pendents d'EF està silenciat, o sigui que l'aplicació
compilarà, arrencarà i només petarà quan una consulta toqui la taula que no existeix.

```bash
# Cal un cop: el manifest té dotnet-ef fixat a 6.0.6 i el projecte va amb EF Core 10
dotnet tool update dotnet-ef

dotnet ef migrations add AfegirNovaEntitat \
  --project DataLayer \
  --startup-project UI.ER.AvaloniaUI
```

Dues regles que no s'han de trencar:

- **No esmenis `20210821093532_inicial`.** Està registrada com a aplicada a totes les
  instal·lacions existents; el que hi afegeixis no hi arribarà mai.
- **No la borris.** Sense ella, EF troba a `__EFMigrationsHistory` una migració aplicada
  que no coneix.

Val per a qualsevol canvi persistit, no només per a una entitat nova: afegir o treure una
propietat, canviar-ne el tipus o tocar una relació. Detall a
[`ARQUITECTURA.md` §6](ARQUITECTURA.md#6-persistència).

### Pas 4: Crear els DTOs d'Entrada

**Ubicació:** `DTO.i/DTOs/`

```csharp
// NovaEntitatCreateParms.cs
namespace DTO.i.DTOs
{
    public class NovaEntitatCreateParms : IDtoi
    {
        public NovaEntitatCreateParms(string codi, string nom, bool esActiu)
        {
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
        }
        public string Codi { get; }
        public string Nom { get; }
        public bool EsActiu { get; }
    }
}

// NovaEntitatUpdateParms.cs
namespace DTO.i.DTOs
{
    public class NovaEntitatUpdateParms : IDtoi, IId
    {
        public NovaEntitatUpdateParms(int id, string codi, string nom, bool esActiu)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
        }
        public int Id { get; }
        public string Codi { get; }
        public string Nom { get; }
        public bool EsActiu { get; }
    }
}
```

### Pas 5: Crear el DTO de Sortida

**Ubicació:** `DTO.o/DTOs/NovaEntitat.cs`

```csharp
using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    public class NovaEntitat : IIdEtiquetaDescripcio, IActiu, IDTOo
    {
        public NovaEntitat(int id, string codi, string nom, bool esActiu, string etiqueta, string descripcio)
        {
            Id = id;
            Codi = codi;
            Nom = nom;
            EsActiu = esActiu;
            Etiqueta = etiqueta;
            Descripcio = descripcio;
        }

        public int Id { get; }
        public string Codi { get; }
        public string Nom { get; }
        public bool EsActiu { get; }
        public string Etiqueta { get; }
        public string Descripcio { get; }
    }
}
```

### Pas 6: Crear la Projecció

**Ubicació:** `DTO.Projections/NovaEntitat.cs`

```csharp
using System;
using System.Linq.Expressions;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace DTO.Projections
{
    public static class NovaEntitat
    {
        public static Expression<Func<Models.NovaEntitat, Dtoo.NovaEntitat>> ToDto
            =>
            model
            =>
            new(
                model.Id, 
                model.Codi, 
                model.Nom, 
                model.EsActiu, 
                model.Etiqueta, 
                model.Descripcio
            );
    }
}
```

### Pas 7: Crear les Interfícies del Servei

**Ubicació:** `BusinessLayer.Abstract/Services/`

```csharp
// INovaEntitatSet.cs
using BusinessLayer.Abstract.Generic;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface INovaEntitatSet : ISet<Parms.EsActiuParms, Dtoo.NovaEntitat>
    {
    }
}

// INovaEntitatCreate.cs
namespace BusinessLayer.Abstract.Services
{
    public interface INovaEntitatCreate : ICreate<Dtoo.NovaEntitat, Parms.NovaEntitatCreateParms>
    {
    }
}

// INovaEntitatUpdate.cs
namespace BusinessLayer.Abstract.Services
{
    public interface INovaEntitatUpdate : IUpdate<Dtoo.NovaEntitat, Parms.NovaEntitatUpdateParms>
    {
    }
}

// INovaEntitatActivaDesactiva.cs
namespace BusinessLayer.Abstract.Services
{
    public interface INovaEntitatActivaDesactiva : IActivaDesactiva<Dtoo.NovaEntitat>
    {
    }
}
```

### Pas 8: Implementar els Serveis

**Ubicació:** `BusinessLayer/Services/`

```csharp
// NovaEntitatSet.cs
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace BusinessLayer.Services
{
    public class NovaEntitatSet :
        BLSet<Models.NovaEntitat, Parms.EsActiuParms, Dtoo.NovaEntitat>,
        INovaEntitatSet
    {
        public NovaEntitatSet(IDbContextFactory<AppDbContext> appDbContextFactory) 
            : base(appDbContextFactory) { }

        protected override IQueryable<Models.NovaEntitat> GetModels(Parms.EsActiuParms request)
            =>
            GetAllModels()
            .Where(i => !request.EsActiu.HasValue || i.EsActiu == request.EsActiu)
            .OrderBy(c => c.Nom);

        protected override Expression<Func<Models.NovaEntitat, Dtoo.NovaEntitat>> ToDto
            => Project.NovaEntitat.ToDto;
    }
}

// NovaEntitatCreate.cs
namespace BusinessLayer.Services
{
    public class NovaEntitatCreate :
        BLCreate<Models.NovaEntitat, Parms.NovaEntitatCreateParms, Dtoo.NovaEntitat>,
        INovaEntitatCreate
    {
        public NovaEntitatCreate(IDbContextFactory<AppDbContext> appDbContextFactory) 
            : base(appDbContextFactory) { }

        protected override Task PreInitialize(NovaEntitatCreateParms parm)
            =>
            new RuleChecker<NovaEntitatCreateParms>(parm)
            .AddCheck(p => string.IsNullOrEmpty(p.Nom), "No es pot deixar el Nom en blanc")
            .AddCheck(RuleEstaRepetit, "Ja existeix una entitat amb aquest nom")
            .Check();

        protected virtual Task<bool> RuleEstaRepetit(NovaEntitatCreateParms parm)
            => GetCollection().AnyAsync(x => x.Nom == parm.Nom);

        protected override Task<Models.NovaEntitat> InitializeModel(NovaEntitatCreateParms parm)
            =>
            Task.FromResult(new Models.NovaEntitat()
            {
                Codi = parm.Codi,
                Nom = parm.Nom,
                EsActiu = parm.EsActiu,
            });

        protected override Task PostAdd(Models.NovaEntitat model, NovaEntitatCreateParms parm)
            => Task.CompletedTask;

        protected override Expression<Func<Models.NovaEntitat, Dtoo.NovaEntitat>> ToDto
            => Project.NovaEntitat.ToDto;
    }
}

// NovaEntitatUpdate.cs
namespace BusinessLayer.Services
{
    public class NovaEntitatUpdate :
        BLUpdate<Models.NovaEntitat, Parms.NovaEntitatUpdateParms, Dtoo.NovaEntitat>,
        INovaEntitatUpdate
    {
        public NovaEntitatUpdate(IDbContextFactory<AppDbContext> appDbContextFactory) 
            : base(appDbContextFactory) { }

        protected override Task PreUpdate(Models.NovaEntitat model, NovaEntitatUpdateParms parm)
            =>
            new RuleChecker<Models.NovaEntitat, NovaEntitatUpdateParms>(model, parm)
            .AddCheck((m, p) => string.IsNullOrEmpty(p.Nom), "No es pot deixar el Nom en blanc")
            .Check();

        protected override Task UpdateModel(Models.NovaEntitat model, NovaEntitatUpdateParms parm)
        {
            model.Codi = parm.Codi;
            model.Nom = parm.Nom;
            model.EsActiu = parm.EsActiu;
            return Task.CompletedTask;
        }

        protected override Task PostUpdate(Models.NovaEntitat model, NovaEntitatUpdateParms parm)
            => Task.CompletedTask;

        protected override void ResetReferences(Models.NovaEntitat model) { }

        protected override Expression<Func<Models.NovaEntitat, Dtoo.NovaEntitat>> ToDto
            => Project.NovaEntitat.ToDto;
    }
}

// NovaEntitatActivaDesactiva.cs
namespace BusinessLayer.Services
{
    public class NovaEntitatActivaDesactiva :
        BLActivaDesactiva<Models.NovaEntitat, Dtoo.NovaEntitat>,
        INovaEntitatActivaDesactiva
    {
        public NovaEntitatActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory) 
            : base(appDbContextFactory) { }

        protected override Task Pre(Models.NovaEntitat model) => Task.CompletedTask;
        protected override Task Post(Models.NovaEntitat model) => Task.CompletedTask;

        protected override Expression<Func<Models.NovaEntitat, Dtoo.NovaEntitat>> ToDto
            => Project.NovaEntitat.ToDto;
    }
}
```

### Pas 9: Registrar al Contenidor DI

**Ubicació:** `BusinessLayer/DI/Injection.cs`

```csharp
// Afegir dins BusinessLayerConfigureServices:
services.AddTransient<INovaEntitatSet, NovaEntitatSet>();
services.AddTransient<INovaEntitatCreate, NovaEntitatCreate>();
services.AddTransient<INovaEntitatUpdate, NovaEntitatUpdate>();
services.AddTransient<INovaEntitatActivaDesactiva, NovaEntitatActivaDesactiva>();
```

#### Els serveis transversals no segueixen aquest camí

Una peça que **no és ni entitat ni operació** —el bus de canvis `INotificadorDeCanvis`, les
dades de l'usuari `IDadesDeLusuari`— es registra diferent:

| | Operació de negoci | Servei transversal |
|---|---|---|
| Contracte | `BusinessLayer.Abstract/Services/IXxx.cs` | `BusinessLayer.Abstract/Generic/IXxx.cs` |
| Implementació | `BusinessLayer/Services/Xxx.cs` | `BusinessLayer/Common/Xxx.cs` |
| Registre | sol, per convenció de nom | `AddSingleton` **a mà** a `BusinessLayerConfigureServices()` |
| Cicle de vida | `Transient` | `Singleton` |
| `IBLOperation`/`IDisposable` | sí, `using var bl = …` | **no**: és de llarga vida |

El namespace no és decoratiu: l'escaneig d'`Injection.Contractes()` filtra per
`BusinessLayer.Abstract.Services`, i un contracte transversal posat allà petaria a
l'arrencada reclamant una implementació que no existeix.

Una operació que necessiti un servei transversal només l'ha de demanar pel constructor
(`ActivatorUtilities` la construeix i el Singleton ja hi és); **no l'ha de cachejar**, perquè
el seu contingut pot canviar mentre l'aplicació és oberta. Perquè hi arribi un ViewModel, en
canvi, cal una propietat nova a `IServiceFactory`.

#### Operació de negoci amb un port cap a l'exterior

Quan una operació ha de sortir de l'ordinador —escriure a un disc que pot no ser-hi, parlar
amb un núvol— el que canvia segons l'exterior **no va dins de l'operació**: va darrere d'un
port, i l'operació es queda comuna. El primer cas és `ICopiaDeSeguretat` amb
`IMagatzemDeCopies`.

| Peça | On viu | Per què |
|---|---|---|
| L'operació (`ICopiaDeSeguretat` → `CopiaDeSeguretat`) | `Abstract/Services/` + `Services/` | És una operació normal: entra pel registre per convenció i es consumeix amb `using var bl = …` |
| El port (`IMagatzemDeCopies`) | `Abstract/Generic/` | **No** a `Services/`: l'escaneig d'operacions filtra per aquell namespace i li reclamaria una implementació de nom `MagatzemDeCopies` |
| Els adaptadors (`MagatzemDeCarpeta`, …) | `BusinessLayer/Common/` | Un `AddSingleton<IMagatzemDeCopies, …>()` a mà per cadascun, **abans** del bucle d'operacions |
| El doble (`MagatzemFals`) | `BusinessLayer.Integration.Test/` | El que fa que l'operació sencera es provi sense xarxa i sense navegador |

Les tres regles que el fan funcionar:

1. **Tot el que és car és comú.** Al cas de les còpies: bolcat, verificació, xifratge,
   retenció i la UI. Darrere del port hi queda «desa aquest fitxer, llista'ls, retira els
   sobrants», que són cinquanta línies per adaptador.
2. **L'operació rep `IEnumerable<TPort>`**, no una implementació concreta. Afegir un destí
   nou és **una línia al registre** i cap canvi a l'operació ni als tests que ja hi ha.
3. **Cap constructor d'adaptador toca res**: ni disc, ni xarxa, ni fitxers de credencials.
   `InjeccioTest.CadaContracteTeLaSevaImplementacioPerConvencio` construeix el contenidor
   sencer i resol totes les operacions en un CI sense res de tot això; tota la feina va
   dins de `Prepara()`.

I com totes les operacions: cap excepció crua cap amunt. Un disc ple, un llapis desendollat
o una carpeta de només lectura han d'arribar a l'usuari com una `BrokenRule` que diu el camí,
no com un `IOException`.

### Pas 10: Crear els ViewModels i Vistes (Opcional)

Seguir el patró existent:
- `NovaEntitatSetViewModel.cs` - Amb `ObservableCollectionExtended<NovaEntitatRowViewModel>`
- `NovaEntitatCreateViewModel.cs` - Amb propietats reactives i `SubmitCommand`
- `NovaEntitatUpdateViewModel.cs` - Similar a Create però carrega dades existents
- `NovaEntitatRowViewModel.cs` - Wrapper del DTO per mostrar a la llista

Vistes AXAML corresponents a `UI.ER.AvaloniaUI/Pages/`.

---

## 5. Sistema de Validacions (Precondicions)

### Ubicació i Estructura

Les validacions es defineixen als mètodes `PreInitialize` (Create) i `PreUpdate` (Update) utilitzant `RuleChecker`:

```csharp
protected override Task PreInitialize(AlumneCreateParms parm)
    =>
    new RuleChecker<AlumneCreateParms>(parm)
    .AddCheck(RuleNoHiHaCapCursActiu, "Abans de crear cal que hi hagi un curs actiu.")
    .AddCheck(RuleEstaRepetit, "Ja existeix un Alumne amb aquests valors")
    .Check();
```

### Tipus de Regles

**Regles síncrones:**
```csharp
// Amb lambda
.AddCheck(p => string.IsNullOrEmpty(p.Nom), "Cal informar el nom")

// Amb mètode
protected virtual bool RuleHiHaValorsNoInformats(CreateParms parm)
    => string.IsNullOrEmpty(parm.Nom);
```

**Regles asíncrones (accés a BD):**
```csharp
// Comprova si ja existeix
protected virtual Task<bool> RuleEstaRepetit(CreateParms parm)
    => GetCollection().AnyAsync(x => x.Nom == parm.Nom);
```

### RuleChecker amb Model (per a Update)

```csharp
new RuleChecker<Models.Alumne, AlumneUpdateParms>(model, parm)
.AddCheck((m, p) => string.IsNullOrEmpty(p.Nom), "Cal informar el nom")
.AddCheck(RuleEstaRepetit, "Ja existeix un altre amb aquests valors")
```

### Gestió d'Errors

Quan una regla falla, es llança `BrokenRuleException` que es captura i es retorna com a `OperationResult` amb `BrokenRules`:

```csharp
catch (BrokenRuleException br)
{
    return new OperationResult<TDTOo>(br.BrokenRules);
}
```

---

## 6. Accions Post Creació/Modificació

### On s'Implementen

- **Post-Create:** `PostAdd(TModel model, TParm parm)` a classes que hereten de `BLCreate`
- **Post-Update:** `PostUpdate(TModel model, TParm parm)` a classes que hereten de `BLUpdate`
- **Post-ActivaDesactiva:** `Post(TModel model)` a classes que hereten de `BLActivaDesactiva`

### Exemple Real: ActuacioCreate

```csharp
protected override async Task PostAdd(Actuacio model, ActuacioCreateParms parm)
{
    // Carregar l'alumne relacionat
    await LoadReference(model, m => m.Alumne);

    // Incrementar comptador
    model.Alumne.NombreTotalDactuacions++;

    // Actualitzar timestamp
    model.Alumne.DataDarreraModificacio = DateTime.Now;
}
```

### Mètodes Auxiliars Disponibles

```csharp
// Carregar una referència (entitat relacionada)
await LoadReference(model, m => m.Relacio);

// Carregar múltiples referències
await LoadReferences(model, m => m.Relacio1, m => m.Relacio2);

// Marcar propietat com modificada (per FK)
ReferencesAreModify(model, x => x.Relacio1, x => x.Relacio2);
```

---

## 7. Convencions de Desenvolupament

### Estructura de Carpetes Habitual

```
{Projecte}/
├── bin/
├── obj/
├── DI/
│   └── Injection.cs          # Configuració DI (si escau)
├── Common/                    # Classes base i utilitats
├── Services/                  # Implementacions de serveis
├── Interfaces/                # Interfícies (BusinessLayer.Abstract)
├── DTOs/                      # Classes DTO (DTO.i, DTO.o)
├── Models/                    # Entitats (DataModels)
├── Configurations/            # Configuracions EF (DataModels.Configuration)
├── ViewModels/                # ViewModels (UI.ER.ViewModels)
├── Pages/                     # Vistes AXAML (UI.ER.AvaloniaUI)
├── Controls/                  # Controls personalitzats (DateInput, LookupInput)
├── Converters/                # Conversors de valors per a bindings
└── Helpers/                   # Utilitats (LogHelpers, WindowHelper)
```

### Convencions d'Alias Imports

```csharp
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Project = DTO.Projections;
using Models = DataModels.Models;
```

### Patrons Repetits a Seguir

1. **Serveis heretant de classes base genèriques**
   - `BLSet<TModel, TParm, TDTOo>` per consultes
   - `BLCreate<TModel, TParm, TDTOo>` per creació
   - `BLUpdate<TModel, TParm, TDTOo>` per actualització
   - `BLActivaDesactiva<TModel, TDTOo>` per activar/desactivar

2. **Propietat `ToDto` obligatòria**
   - Tots els serveis han de definir `Expression<Func<TModel, TDTOo>> ToDto`

3. **Constructor amb IDbContextFactory**
   - Tots els serveis reben `IDbContextFactory<AppDbContext>` via DI

4. **ViewModels amb ReactiveUI**
   - Hereten de `ViewModelBase` (que és `ReactiveValidationObject`)
   - Usen `RaiseAndSetIfChanged` per propietats
   - Usen `ReactiveCommand` per accions
   - Usen `Interaction<,>` per diàlegs

5. **DTOs immutables**
   - Propietats de només lectura (`{ get; }`)
   - Valors assignats al constructor

### Decisions de Disseny Implícites

1. **Entitats sempre tenen `Id` enter**
2. **La majoria d'entitats tenen `EsActiu` per soft-delete**
3. **`Etiqueta` i `Descripcio` es calculen a l'entitat** per mostrar-se a la UI
4. **Els DTOs d'Update inclouen `IId`** per identificar l'entitat
5. **Les validacions es fan ABANS de modificar** (fail-fast)
6. **El context es crea per operació** (via Factory pattern)
7. **Els serveis són `Transient`** (una instància per ús), tret dels transversals —el bus i
   les dades de l'usuari—, que són `Singleton`
8. **Les migracions s'*apliquen* automàticament** a l'inici, però **generar-les és manual i
   ningú no t'ho recorda** (§3 i Pas 3)

### Exemple d'Ús des de ViewModel

El ViewModel rep la fàbrica de serveis pel constructor i la guarda a `_serveis`; no hi ha
cap manera estàtica d'arribar al BusinessLayer (§R1 de `refactors.md`).

```csharp
public class AlumneSetViewModel : ViewModelBase
{
    private readonly IServiceFactory _serveis;

    public AlumneSetViewModel(IServiceFactory serveis, bool modeLookup = false) { … }

    private async Task Carrega()
    {
        // Obtenir servei: una instància nova per crida, i es disposa en sortir de l'àmbit
        using var bl = _serveis.GetBLOperation<IAlumneSet>();

        // Executar operació
        var dto = await bl.FromPredicate(new AlumneSearchParms(esActiu: true));

        // Gestionar resultat
        if (dto.BrokenRules.Any())
        {
            // Mostrar errors
            BrokenRules.AddRange(dto.BrokenRules.Select(r => r.Message));
        }
        else
        {
            // Processar dades. El ViewModel fill també necessita la fàbrica: se li passa.
            var items = dto.Data.Select(d => new AlumneRowViewModel(_serveis, d, cursActual));
        }
    }
}
```

### Exemple d'Ús de BLReport

```csharp
// Servei que genera un informe Word
public class AlumneInforme : BLReport<SaveResult>, IAlumneInforme
{
    public Task<OperationResult<SaveResult>> Run(int alumneId)
        => ExecuteReport(() => GenerateReport(alumneId));

    private async Task<SaveResult> GenerateReport(int alumneId)
    {
        var dades = await GetDadesAlumne(alumneId);
        if (dades == null) throw new BrokenRuleException("Alumne no trobat");

        var (path, filename, folder) = CalculatePath("informe_alumne", "docx");
        // ... generar document ...
        return new SaveResult(path, filename, folder);
    }
}
```

### Exemple d'Ús de BLBatchOperation

```csharp
// Servei que modifica múltiples registres
public class AlumneSyncActiuByCentre : BLBatchOperation<EtiquetaDescripcio>, IAlumneSyncActiuByCentre
{
    public Task<OperationResult<EtiquetaDescripcio>> Run()
        => ExecuteBatch(SyncAlumnes);

    private async Task<EtiquetaDescripcio> SyncAlumnes()
    {
        // ... lògica de sincronització ...
        return new EtiquetaDescripcio(etiqueta: "Resultat", descripcio: "Detalls");
    }
}
```

---

## 8. Llibreries i Dependències

### UI
| Paquet | Versió |
|--------|--------|
| Avalonia | 11.3.11 |
| Avalonia.Desktop | 11.3.11 |
| Avalonia.Diagnostics | 11.3.11 |
| Material.Avalonia | 3.13.4 |
| Material.Icons.Avalonia | 2.4.1 |
| ReactiveUI.Avalonia | 11.3.8 |
| Serilog.Sinks.File | 7.0.0 |

* Material Avalonia: https://github.com/AvaloniaCommunity/Material.Avalonia

### Dades
| Paquet | Versió |
|--------|--------|
| Microsoft.EntityFrameworkCore | 10.0.2 |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.2 |
| Microsoft.EntityFrameworkCore.Design | 10.0.2 |
| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.2 |

### Negoci / Informes
| Paquet | Versió |
|--------|--------|
| ClosedXML | 0.105.0 |
| EPPlus | 8.4.2 |
| SharpDocx | 2.6.0 |

### Logging
| Paquet | Versió |
|--------|--------|
| Serilog.Sinks.File | 7.0.0 |

El logging d'errors està configurat a `UI.ER.AvaloniaUI/Helpers/LogHelpers.cs`. Escriu a `error.log` al directori de l'executable. Captura:
- Excepcions no gestionades de `AppDomain`
- Excepcions de tasques no observades (`TaskScheduler`)
- Excepcions de pipelines ReactiveUI (`RxApp`)
- Excepcions fatals a l'arrencada/aturada de l'aplicació

---


## Resum

Aquesta aplicació és un sistema de gestió educativa que segueix una arquitectura neta per capes amb:

- **Capa UI:** Avalonia UI + MVVM amb ReactiveUI
- **Capa Business:** Serveis genèrics amb validació integrada
- **Capa Data:** EF Core amb SQLite

Per afegir noves funcionalitats, segueix l'ordre: Entitat → Configuració → DTO.i → DTO.o → Projecció → Interfícies → Serveis → DI → ViewModels → Vistes.

# EapRecull - Guia de Context per a Agents IA

> Document generat per proporcionar tot el context necessari per entendre, mantenir i ampliar aquesta aplicació.

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
| **UI.ER.AvaloniaUI** | Vistes AXAML i code-behind |

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
| `IModel` | Marcador per a entitats persistides |
| `IDtoi` | Marcador per a DTOs d'entrada |
| `IDTOo` | Marcador per a DTOs de sortida |

### Convencions de Noms

**DTOs d'entrada (`DTO.i`):**
- `{Entitat}CreateParms` - Per crear noves entitats
- `{Entitat}UpdateParms` - Per actualitzar entitats (inclou `IId`)
- `{Entitat}SearchParms` - Per cercar/filtrar entitats
- `EsActiuParms` - Paràmetre genèric per filtrar per `EsActiu`
- `EmptyParms` - Quan no cal cap paràmetre

**DTOs de sortida (`DTO.o`):**
- Mateix nom que l'entitat: `Alumne`, `Centre`, `Actuacio`, etc.

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
- **Migracions:** Automàtiques a l'inici via `Database.Migrate()`

---

## 4. Guia per Crear un CRUD Complet

### Pas 1: Crear l'Entitat

**Ubicació:** `DataModels/Models/{Entitat}.cs`

```csharp
using CommonInterfaces;
using DataModels.Models.Interfaces;

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

### Pas 3: Registrar al DbContext

**Ubicació:** `DataLayer/AppDbContext.cs`

```csharp
public virtual DbSet<NovaEntitat> NovesEntitats => Set<NovaEntitat>();
```

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
└── Pages/                     # Vistes AXAML (UI.ER.AvaloniaUI)
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
7. **Els serveis són `Transient`** (una instància per ús)
8. **Les migracions s'executen automàticament** a l'inici

### Exemple d'Ús des de ViewModel

```csharp
// Obtenir servei
using var bl = SuperContext.GetBLOperation<IAlumneSet>();

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
    // Processar dades
    var items = dto.Data.Select(d => new AlumneRowViewModel(d));
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

## Resum

Aquesta aplicació és un sistema de gestió educativa que segueix una arquitectura neta per capes amb:

- **Capa UI:** Avalonia UI + MVVM amb ReactiveUI
- **Capa Business:** Serveis genèrics amb validació integrada
- **Capa Data:** EF Core amb SQLite

Per afegir noves funcionalitats, segueix l'ordre: Entitat → Configuració → DTO.i → DTO.o → Projecció → Interfícies → Serveis → DI → ViewModels → Vistes.

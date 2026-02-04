# Guia d'Actualització: .NET 6.0 → .NET 10.0

---

## 📋 Anàlisi del Patró MVVM

### Arquitectura General
L'aplicació segueix el patró **MVVM amb ReactiveUI**, separant:
- **Models**: DTOs a `DTO.o` i `DTO.i`
- **Views**: Fitxers `.axaml` i `.axaml.cs` a `UI.ER.AvaloniaUI/Pages/`
- **ViewModels**: Classes a `UI.ER.ViewModels/ViewModels/`

### Estructura per Entitat
Cada entitat (Alumne, Centre, Etapa, etc.) segueix un patró consistent de 4 ViewModels:

| Tipus | Funció | Exemple |
|-------|--------|---------|
| `{Entitat}SetViewModel` | Llista amb filtres i paginació | `AlumneSetViewModel` |
| `{Entitat}RowViewModel` | Representa una fila de la llista | `AlumneRowViewModel` |
| `{Entitat}CreateViewModel` | Formulari de creació | `AlumneCreateViewModel` |
| `{Entitat}UpdateViewModel` | Formulari d'edició | `AlumneUpdateViewModel` |

### ✅ Punts Positius (Ben Implementat)

1. **Separació correcta View/ViewModel**
   - Views són `ReactiveWindow<TViewModel>` o `ReactiveUserControl<TViewModel>`
   - ViewModels hereten de `ViewModelBase` (que hereta de `ReactiveValidationObject`)

2. **Ús correcte de ReactiveUI**
   - Propietats reactives amb `RaiseAndSetIfChanged`
   - Commands amb `ReactiveCommand`
   - Observables amb `WhenAnyValue` i `CombineLatest`
   - Validacions amb `ReactiveUI.Validation`

3. **Interaccions per diàlegs**
   - Ús de `Interaction<TInput, TOutput>` per obrir finestres modals
   - Registre correcte amb `RegisterHandler` al `WhenActivated`

4. **Injecció de Dependències**
   - `SuperContext` com a Service Locator per accedir al BusinessLayer
   - Serveis registrats via `ServiceCollection`

5. **Gestió d'errors**
   - `BrokenRules` col·lecció observable per mostrar errors
   - Conversió DTO a ViewModel amb `DTO2ModelView`

### ⚠️ Inconsistències Detectades

#### 1. ViewLocator no s'utilitza
```csharp
// ViewLocator.cs intenta resoldre "ViewModel" → "View"
var name = data.GetType().FullName!.Replace("ViewModel", "View");
```
**Problema**: El ViewLocator assumeix que les Views estan al mateix namespace que els ViewModels, però:
- ViewModels: `UI.ER.ViewModels.ViewModels`
- Views: `UI.ER.AvaloniaUI.Pages`

**Impacte**: Baix - les Views es creen manualment als diàlegs.

#### 2. Namespaces inconsistents
- ViewModels al namespace `UI.ER.ViewModels.ViewModels` (duplicat)
- Services al namespace `UI.ER.AvaloniaUI.Services` però al projecte `UI.ER.ViewModels`

**Fitxers afectats**:
- `SuperContext.cs` → namespace `UI.ER.AvaloniaUI.Services`
- `StringDateConverter.cs` → hauria d'estar amb els ViewModels

#### 3. Design DataContext incorrectes als AXAML
```xml
<!-- AlumneSetWindow.axaml -->
<Design.DataContext>
    <viewModels:AlumnesViewModel />  <!-- No existeix! -->
</Design.DataContext>

<!-- CentreSetWindow.axaml -->
<Design.DataContext>
    <viewModels:CentresViewModel />  <!-- No existeix! -->
</Design.DataContext>
```
**Hauria de ser**: `AlumneSetViewModel` i `CentreSetViewModel`

#### 4. Propietats amb backing field inconsistent
```csharp
// Alguns usen guió baix
public string _Nom = string.Empty;  // ❌ Hauria de ser privat

// Correcte seria:
private string _nom = string.Empty;
public string Nom { get => _nom; set => ... }
```

#### 5. Loading state no consistent
- `AlumneSetViewModel` té `Loading` property ✅
- `ActuacioSetViewModel` té `Loading` property ✅
- `CentreSetViewModel` **NO** té `Loading` property ❌
- `EtapaSetViewModel` **NO** té `Loading` property ❌

#### 6. Alguns ViewModels no desregistren subscripcions
Els ViewModels no implementen `IDisposable` per netejar subscripcions.

#### 7. Codi duplicat als Lookups
Cada `CreateWindow.axaml.cs` i `UpdateWindow.axaml.cs` repeteix els mateixos handlers de Lookup:
```csharp
private async Task CentreLookupShowDialogAsync(...) { ... }
private async Task EtapaActualLookupShowDialogAsync(...) { ... }
```

### 📊 Resum de Consistència per Entitat

| Entitat | Set | Row | Create | Update | Consistència |
|---------|-----|-----|--------|--------|--------------|
| Alumne | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |
| Centre | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |
| Etapa | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |
| TipusActuacio | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |
| CursAcademic | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |
| Actuacio | ✅ | ✅ | ✅ | ✅ | 🟢 Alta |

### 🔧 Recomanacions de Millora (Opcional)

1. **Corregir Design.DataContext** als AXAML per tenir millor suport al designer
2. **Uniformitzar Loading state** a tots els SetViewModels
3. **Crear una classe base** per als Lookup handlers i evitar duplicació
4. **Moure SuperContext** al namespace correcte `UI.ER.ViewModels.Services`
5. **Fer privats els backing fields** (`_Nom` → `private string _nom`)

### ✅ Conclusió

El patró MVVM està **ben implementat i és consistent** entre totes les entitats. Les inconsistències detectades són menors i no bloquegen la migració. Es poden arreglar durant o després de l'actualització de .NET.

---

## Resum de l'Estat Actual

### Versions Actuals ✅ MIGRAT
- **Target Framework**: `net10.0` (tots els projectes)
- **Avalonia UI**: `11.3.11`
- **Material.Avalonia**: `3.13.4`
- **Entity Framework Core**: `10.0.2`
- **ReactiveUI**: `22.2.1` (via ReactiveUI.Avalonia)
- **ReactiveUI.Validation**: `6.0.18`

### Projectes de la Solució (15 projectes)
| Projecte | Tipus | Dependències Crítiques |
|----------|-------|----------------------|
| UI.ER.AvaloniaUI | WinExe | Avalonia 0.10.15, Material.Avalonia |
| UI.ER.ViewModels | Library | ReactiveUI 18.2.5 |
| BusinessLayer | Library | EF Core 6.0.6, ClosedXML, EPPlus, SharpDocx |
| DataLayer | Library | EF Core SQLite 6.0.6 |
| DataModels | Library | - |
| DataModels.Configuration | Library | EF Core 6.0.6 |
| BusinessLayer.Abstract | Library | - |
| CommonInterfaces | Library | - |
| DTO.i | Library | - |
| DTO.o | Library | - |
| DTO.Projections | Library | - |
| CreateDemoData | Exe | - |
| ImportData | Exe | - |
| CmdHello | Exe | Terminal.Gui 1.6.4 |
| BusinessLayer.Integration.Test | Test | xUnit 2.4.1 |

---

## ⚠️ Canvis Crítics (Breaking Changes)

### 1. Avalonia UI: 0.10.x → 11.x
**Risc: ALT** 🔴

L'actualització d'Avalonia és el canvi més significatiu. La versió 11 és una reescriptura major.

#### Canvis Principals:
- **Nou sistema de temes**: `Fluent` i `Simple` themes substitueixen l'antic sistema
- **Canvis en XAML namespaces**
- **API de controls modificada**
- **Nou sistema de styling**
- **Canvis en el cicle de vida de l'aplicació**

#### Fitxers Afectats:
- `App.axaml` - Cal reescriure completament
- Tots els fitxers `.axaml` a `UI.ER.AvaloniaUI/Pages/` i `UI.ER.AvaloniaUI/Views/`
- `Program.cs` - Nou builder pattern

### 2. Material.Avalonia → Material.Avalonia 3.x (estable) o alternativa
**Risc: ALT** 🔴

- La versió `3.0.0-rc0.92-nightly` que fas servir és incompatible amb Avalonia 11
- Cal migrar a `Material.Avalonia` compatible amb Avalonia 11 o considerar alternatives

**Opcions:**
1. **Material.Avalonia** per Avalonia 11 (si disponible)
2. **Semi.Avalonia** - Tema modern alternatiu
3. **Fluent Theme natiu** d'Avalonia 11

### 3. Entity Framework Core: 6.0 → 10.0
**Risc: MITJÀ** 🟡

- Possibles canvis en migracions
- Alguns mètodes obsolets eliminats
- Millores en performance

### 4. ReactiveUI: 18.x → 20.x
**Risc: BAIX-MITJÀ** 🟡

- Alguns canvis d'API
- Millor integració amb Avalonia 11

---

## 📋 Tasques d'Actualització

### Fase 0: Preparació ✅
- [x] Fer backup complet del projecte
- [x] Crear branca `upgradeToNow`
- [x] Assegurar que tots els tests passen amb la versió actual (3/3 OK)
- [x] Instal·lar .NET 10 SDK (10.0.102 disponible)

### Fase 1: Actualitzar Target Framework (Tots els Projectes)

Canviar `<TargetFramework>net6.0</TargetFramework>` a `<TargetFramework>net10.0</TargetFramework>` en:

```
CommonInterfaces/CommonInterfaces.csproj
DataModels/DataModels.csproj
DTO.i/DTO.i.csproj
DTO.o/DTO.o.csproj
DTO.Projections/DTO.Projections.csproj
DataModels.Configuration/DataModels.Configuration.csproj
DataLayer/DataLayer.csproj
BusinessLayer.Abstract/BusinessLayer.Abstract.csproj
BusinessLayer/BusinessLayer.csproj
UI.ER.ViewModels/UI.ER.ViewModels.csproj
UI.ER.AvaloniaUI/UI.ER.AvaloniaUI.csproj
CreateDemoData/CreateDemoData.csproj
ImportData/ImportData.csproj
CmdHello/CmdHello.csproj
BusinessLayer.Integration.Test/BusinessLayer.Integration.Test.csproj
```

### Fase 2: Actualitzar Paquets NuGet (Backend)

#### DataLayer & DataModels.Configuration
```xml
<!-- De -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.6" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.0" />

<!-- A (verificat NuGet 04/02/2026) -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.2" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="3.0.2" />
```

#### BusinessLayer
```xml
<!-- De -->
<PackageReference Include="ClosedXML" Version="0.96.0" />
<PackageReference Include="EPPlus" Version="6.0.5" />
<PackageReference Include="SharpDocx" Version="2.2.0" />

<!-- A (verificat NuGet 04/02/2026) -->
<PackageReference Include="ClosedXML" Version="0.105.0" />
<PackageReference Include="EPPlus" Version="8.4.2" />
<PackageReference Include="SharpDocx" Version="2.6.0" />
```

#### BusinessLayer.Integration.Test
```xml
<!-- De -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.2.0" />
<PackageReference Include="xunit" Version="2.4.1" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.3" />
<PackageReference Include="coverlet.collector" Version="3.1.2" />

<!-- A (verificat NuGet 04/02/2026) -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

### Fase 3: Actualitzar Capa UI (⚠️ Més Complex)

#### UI.ER.AvaloniaUI.csproj
```xml
<!-- De -->
<PackageReference Include="Avalonia" Version="0.10.15" />
<PackageReference Include="Avalonia.Desktop" Version="0.10.15" />
<PackageReference Include="Avalonia.Diagnostics" Version="0.10.15" />
<PackageReference Include="Avalonia.ReactiveUI" Version="0.10.15" />
<PackageReference Include="Material.Icons.Avalonia" Version="1.0.2" />
<PackageReference Include="Material.Avalonia" Version="3.0.0-rc0.92-nightly" />

<!-- A (APLICAT 04/02/2026) - Amb Material.Avalonia -->
<!-- ⚠️ IMPORTANT: Avalonia.ReactiveUI està DEPRECAT, usar ReactiveUI.Avalonia -->
<PackageReference Include="Avalonia" Version="11.3.11" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.11" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.3.11" />
<PackageReference Include="ReactiveUI.Avalonia" Version="11.3.8" />
<PackageReference Include="Material.Icons.Avalonia" Version="2.4.1" />
<PackageReference Include="Material.Avalonia" Version="3.13.4" />

<!-- A (Opció 2: Amb Fluent Theme natiu) -->
<PackageReference Include="Avalonia" Version="11.3.11" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.11" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.11" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.11" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.3.11" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.3.11" />
<PackageReference Include="Material.Icons.Avalonia" Version="2.4.1" />
```

#### UI.ER.ViewModels.csproj
```xml
<!-- De -->
<PackageReference Include="ReactiveUI" Version="18.2.5" />
<PackageReference Include="ReactiveUI.Validation" Version="3.0.1" />

<!-- A (APLICAT 04/02/2026) -->
<!-- ⚠️ ReactiveUI ja no cal explícitament, ve transitiu via ReactiveUI.Validation -->
<PackageReference Include="ReactiveUI.Validation" Version="6.0.18" />
```

### Fase 4: Migrar Codi Avalonia

#### 4.1 Program.cs - Nou Builder Pattern
```csharp
// Abans (Avalonia 0.10)
public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI();

// Després (Avalonia 11)
public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseReactiveUI();
```

#### 4.2 App.axaml - Nou Sistema de Temes

**Opció 1: Material.Avalonia (mantenir estil actual)**
```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="UI.ER.AvaloniaUI.App"
             RequestedThemeVariant="Light">
    <Application.Styles>
        <StyleInclude Source="avares://Material.Avalonia/Material.Avalonia.Templates.xaml" />
    </Application.Styles>
</Application>
```

**Opció 2: Fluent Theme natiu**
```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="UI.ER.AvaloniaUI.App"
             RequestedThemeVariant="Light">
    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
```

#### 4.3 Canvis en Controls XAML

| Avalonia 0.10 | Avalonia 11 |
|---------------|-------------|
| `Window.HasSystemDecorations` | `Window.SystemDecorations` |
| `TextBlock.TextWrapping="Wrap"` | Igual |
| `Binding Path=...` | Generalment igual, però revisar |
| `classes:` pseudo-classes | `(classes)` sintaxi |

#### 4.4 Revisar Tots els Fitxers AXAML
- [ ] `UI.ER.AvaloniaUI/Pages/*.axaml`
- [ ] `UI.ER.AvaloniaUI/Views/*.axaml`
- [ ] Actualitzar namespaces si cal
- [ ] Revisar bindings i converters

### Fase 5: Compilar i Resoldre Errors

```bash
# Netejar i reconstruir
dotnet clean
dotnet restore
dotnet build

# Executar tests
dotnet test
```

### Fase 6: Provar l'Aplicació

- [ ] Verificar que l'aplicació arrenca
- [ ] Provar totes les pantalles/vistes
- [ ] Verificar operacions CRUD
- [ ] Provar exportacions (Excel, Word)
- [ ] Verificar que els estils es veuen correctament

---

## 🔧 Comandes Útils

```bash
# Verificar versió de .NET instal·lada
dotnet --list-sdks

# Actualitzar tots els paquets NuGet
dotnet outdated  # (requereix eina dotnet-outdated)

# Actualitzar paquet específic
dotnet add package NomPaquet --version X.Y.Z

# Netejar completament
dotnet clean
rm -rf */bin */obj

# Restaurar i compilar
dotnet restore
dotnet build
```

---

## 📚 Recursos de Migració

### Avalonia
- [Guia oficial de migració 0.10 → 11](https://docs.avaloniaui.net/docs/stay-up-to-date/upgrade-from-0.10)
- [Breaking changes Avalonia 11](https://github.com/AvaloniaUI/Avalonia/wiki/Breaking-Changes)
- [Material.Avalonia Wiki](https://github.com/AvaloniaCommunity/Material.Avalonia/wiki)

### Entity Framework Core
- [EF Core Breaking Changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/)

### ReactiveUI
- [ReactiveUI Release Notes](https://github.com/reactiveui/ReactiveUI/releases)

### .NET
- [.NET 10 What's New](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)

---

## ⏱️ Estimació de Temps

| Fase | Temps Estimat |
|------|---------------|
| Fase 0: Preparació | 30 min |
| Fase 1: Target Framework | 15 min |
| Fase 2: Paquets Backend | 1-2 hores |
| Fase 3: Paquets UI | 30 min |
| Fase 4: Migrar Codi Avalonia | 4-8 hores ⚠️ |
| Fase 5: Resoldre Errors | 2-4 hores |
| Fase 6: Testing | 2-3 hores |
| **Total** | **10-18 hores** |

---

## 🎯 Recomanacions

1. **Fer la migració incrementalment**: Primer backend, després UI
2. **Considerar migrar a Avalonia 11 amb Fluent Theme** si Material.Avalonia dona problemes
3. **Mantenir la branca original** fins que tot funcioni
4. **Escriure tests** per les funcionalitats crítiques abans de migrar
5. **Actualitzar els fitxers AXAML un per un**, comprovant que compilen

---

## ✅ Checklist Final

- [x] Tots els projectes compilen sense errors
- [x] Tots els tests passen (3/3)
- [x] L'aplicació arrenca correctament
- [ ] La interfície es veu correctament
- [ ] Les operacions de dades funcionen
- [ ] Les exportacions funcionen
- [ ] No hi ha warnings crítics (hi ha CS8981 warnings per noms amb minúscules)

---

## 📦 Migració Completada (4 febrer 2026)

### Canvis Realitzats

#### 1. Target Framework
Migrat de `net6.0` a `net10.0` a tots els 15 projectes.

#### 2. Canvi Crític: Avalonia.ReactiveUI → ReactiveUI.Avalonia

El paquet `Avalonia.ReactiveUI` ha estat **DEPRECAT** i substituït per `ReactiveUI.Avalonia`:

```xml
<!-- ABANS (DEPRECATED) -->
<PackageReference Include="Avalonia.ReactiveUI" Version="..." />

<!-- ARA -->
<PackageReference Include="ReactiveUI.Avalonia" Version="11.3.8" />
```

**Canvi de namespace necessari** a tots els fitxers `.cs`:
```csharp
// Abans
using Avalonia.ReactiveUI;

// Ara
using ReactiveUI.Avalonia;
```

#### 3. Paquets Actualitzats

| Paquet | Versió Anterior | Versió Nova |
|--------|-----------------|-------------|
| Avalonia.* | 0.10.15 | 11.3.11 |
| Material.Avalonia | 3.0.0-rc0.92 | 3.13.4 |
| Material.Icons.Avalonia | 1.0.2 | 2.4.1 |
| Microsoft.EntityFrameworkCore.* | 6.0.6 | 10.0.2 |
| xUnit | 2.4.1 | 2.9.3 |
| xUnit.runner.visualstudio | 2.4.3 | 3.1.5 |
| Microsoft.NET.Test.Sdk | 16.11.0 | 17.14.1 |
| ReactiveUI.Validation | 3.0.1 | 6.0.18 |

### Problemes Trobats i Resolts

1. **Runtime error `MissingMethodException` amb DynamicData.IObservableList.Items**
   - **Causa**: Versió incompatible de DynamicData amb ReactiveUI antic
   - **Solució**: Usar el nou paquet `ReactiveUI.Avalonia` que gestiona les dependències correctament

2. **Runtime error `TypeLoadException` amb Splat.IEnableLogger**
   - **Causa**: `ReactiveUI.Validation 4.0.9` incompatible amb Splat 17.x
   - **Solució**: Actualitzar `ReactiveUI.Validation` a 6.0.18

3. **Build errors amb namespace `Avalonia.ReactiveUI`**
   - **Causa**: El nou paquet `ReactiveUI.Avalonia` usa namespace diferent
   - **Solució**: Canviar totes les importacions de `using Avalonia.ReactiveUI;` a `using ReactiveUI.Avalonia;`

### Fitxers Modificats

- `UI.ER.AvaloniaUI/UI.ER.AvaloniaUI.csproj` - Canvi de paquets
- `UI.ER.ViewModels/UI.ER.ViewModels.csproj` - Actualització ReactiveUI.Validation
- `UI.ER.AvaloniaUI/Program.cs` - Canvi de namespace
- 26 fitxers `.axaml.cs` - Canvi de `using Avalonia.ReactiveUI` a `using ReactiveUI.Avalonia`

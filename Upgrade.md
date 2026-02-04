# Guia d'Actualització: .NET 6.0 → .NET 10.0

## Resum de l'Estat Actual

### Versions Actuals
- **Target Framework**: `net6.0` (tots els projectes)
- **Avalonia UI**: `0.10.15` ⚠️ Molt antiga (actual: 11.x)
- **Material.Avalonia**: `3.0.0-rc0.92-nightly` ⚠️ Versió nightly antiga
- **Entity Framework Core**: `6.0.6`
- **ReactiveUI**: `18.2.5`

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

### Fase 0: Preparació
- [ ] Fer backup complet del projecte
- [ ] Crear branca `upgradeToNow` ✅ (fet)
- [ ] Assegurar que tots els tests passen amb la versió actual
- [ ] Instal·lar .NET 10 SDK

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

<!-- A -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.6" />
```

#### BusinessLayer
```xml
<!-- De -->
<PackageReference Include="ClosedXML" Version="0.96.0" />
<PackageReference Include="EPPlus" Version="6.0.5" />
<PackageReference Include="SharpDocx" Version="2.2.0" />

<!-- A (verificar últimes versions) -->
<PackageReference Include="ClosedXML" Version="0.104.1" />
<PackageReference Include="EPPlus" Version="7.5.2" />
<PackageReference Include="SharpDocx" Version="2.4.0" />
```

#### BusinessLayer.Integration.Test
```xml
<!-- De -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.2.0" />
<PackageReference Include="xunit" Version="2.4.1" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.3" />
<PackageReference Include="coverlet.collector" Version="3.1.2" />

<!-- A -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```

#### CmdHello
```xml
<!-- De -->
<PackageReference Include="Terminal.Gui" Version="1.6.4" />

<!-- A -->
<PackageReference Include="Terminal.Gui" Version="2.0.0" />
```
⚠️ Terminal.Gui v2 té breaking changes significatius

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

<!-- A (Opció 1: Amb Material.Avalonia) -->
<PackageReference Include="Avalonia" Version="11.2.3" />
<PackageReference Include="Avalonia.Desktop" Version="11.2.3" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.2.3" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.2.3" />
<PackageReference Include="Material.Icons.Avalonia" Version="2.1.10" />
<PackageReference Include="Material.Avalonia" Version="3.9.1" />

<!-- A (Opció 2: Amb Fluent Theme natiu) -->
<PackageReference Include="Avalonia" Version="11.2.3" />
<PackageReference Include="Avalonia.Desktop" Version="11.2.3" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.3" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.2.3" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.2.3" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.2.3" />
<PackageReference Include="Material.Icons.Avalonia" Version="2.1.10" />
```

#### UI.ER.ViewModels.csproj
```xml
<!-- De -->
<PackageReference Include="ReactiveUI" Version="18.2.5" />
<PackageReference Include="ReactiveUI.Validation" Version="3.0.1" />

<!-- A -->
<PackageReference Include="ReactiveUI" Version="20.1.63" />
<PackageReference Include="ReactiveUI.Validation" Version="4.0.9" />
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

- [ ] Tots els projectes compilen sense errors
- [ ] Tots els tests passen
- [ ] L'aplicació arrenca correctament
- [ ] La interfície es veu correctament
- [ ] Les operacions de dades funcionen
- [ ] Les exportacions funcionen
- [ ] No hi ha warnings crítics

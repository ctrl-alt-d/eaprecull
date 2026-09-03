# Refactors pendents — UI.ER.AvaloniaUI

> **Propòsit**: document de context per a agents IA durant el refactor de la capa de presentació.
> Complementa `agents.md` (que descriu l'arquitectura *actual* de tota la solució).
> Branca de treball: `refactorDI`.

## Estat del pla

| Refactor | Estat |
|---|---|
| **R0** — `IWindowFactory` + registre per comprensió | ✅ **FET** — veure §R0 |
| **R3** — classes base + helpers | ✅ **FET** — veure §R3 |
| **R2** — disposal de subscripcions | ✅ **FET dins de R3**, tret d'`AlumneInformeViewerWindow` — veure §R2 |
| **R5** — codi mort | ✅ **FET** — veure §R5 |
| **R4** — navegació de `MainWindow` | ✅ **FET** — veure §R4 |
| **R6** — unificació visual i paleta | ✅ **FET** — veure §R6 |
| **R1** — eliminar `SuperContext` | ✅ **FET** — veure §R1 |
| **R7** — registre del BusinessLayer per comprensió | ✅ **FET** — veure §R7 |
| R8 — estendre els *compiled bindings* a l'AXAML que R1 no ha tocat | ⬜ **següent** — veure §R1.7 |

---

## 1. Estat actual de `UI.ER.AvaloniaUI`

**Volum després de R3, R5, R4, R6 i R1**: el codi rere les vistes (`*.axaml.cs`) ha passat de
**2.233 a 1.164 línies** (−1.056 de R3/R5/R4, +62 de R6: dues vistes noves, −17 de R1: els 16
`Func<ViewModelBase>` dels lookups), a canvi de **407 línies** de codi compartit: tres classes base
(`Pages/Base/`), tres helpers (`Helpers/{DialegExtensions,FileExplorer,VisualRootExtensions}.cs`)
i les tres interfícies de contracte dels ViewModels
(`UI.ER.ViewModels/ViewModels/Contracts/DialegContracts.cs`).
Tot el projecte suma **2.053 línies** de `.cs`.

L'AXAML: **33 fitxers, 3.016 línies**. R6 va deixar els **96 colors literals** a **0**; R1 hi ha
tret els **14 blocs `<Design.DataContext>`** i n'ha posat `x:DataType` + `x:CompileBindings="True"`
(§R1.5). Els **19 fitxers restants** encara van amb bindings per reflexió — R8.

`UI.ER.ViewModels`: **5.144 línies** de `.cs` (5.089 abans de R1). R1 hi ha afegit `Services/ServiceFactory.cs` i
l'`IServiceFactory` al constructor de **27 ViewModels**; hi ha esborrat `Services/SuperContext.cs`
i els 4 *seams* `protected virtual IXxx BLxxx()`.

`UI.ER.AvaloniaUI.Test`: 9 fitxers de `.cs` (8 de tests + `Vistes.cs`), **40 tests**.
`BusinessLayer.Integration.Test`: 3 fitxers, **6 tests** (3 abans de R7).

### Patrons en ús

| Patró | On es materialitza |
|---|---|
| **MVVM + ReactiveUI** | `ReactiveWindow<TVm>` / `ReactiveUserControl<TVm>` + `WhenActivated` |
| **Interaction pattern** | `vm.ShowXDialog.RegisterHandler(...)` — el VM demana un diàleg sense conèixer Avalonia |
| **Navegació per comanda** | `Command="{Binding XSetCommand}"` a l'AXAML + `RegistraNavegacio<XWindow>` a la vista (R4) |
| **Composition Root** | `App.OnFrameworkInitializationCompleted` → `DataLayerConfigureServices()` + `BusinessLayerConfigureServices()` + `UIConfigureServices()` |
| **Factory de vistes** | `IWindowFactory.Get<T>(vmArgs)` / `GetWith<T>(dc)` — cap `new XWindow(` al codi (R0, R1) |
| **Classes base genèriques** | `EntityEditWindow<TVm,TDto>`, `EntitySetWindow<…>`, `EntityRowUserCtrl<…>` a `Pages/Base/` (R3) |
| **Contractes de ViewModel** | `ISubmitViewModel<TDto>`, `ISetViewModel<…>`, `IRowViewModel<…>` — el que les classes base poden donar per fet (R3) |
| **Registre per comprensió** | `UI.ER.AvaloniaUI/DI/Injection.cs` escaneja finestres i ViewModels |
| **Fàbrica de serveis injectada** | `IServiceFactory.GetBLOperation<T>()` — **51 crides**, sempre des d'un camp `_serveis` rebut pel constructor (R1) |
| **Compiled bindings** | `x:DataType` + `x:CompileBindings="True"` als 14 fitxers que abans tenien `<Design.DataContext>` (R1) |
| **Attached behavior** | `WindowHelper.ClampToWorkingArea` aplicada globalment amb `Style Selector="Window"` |
| **Custom controls** | `DateInput`, `LookupInput` amb `StyledProperty` |
| **Styling per classes** | `LookupCss`, `ClearCss`, `EditarCss`, `Fitxa`, `BarraFiltres`, `Xip`, `Seccio{,.Ok,.Avis,.Perill}`, `Desar`, `Afegir`… tot a `App.axaml` (R6) |
| **Paleta semàntica** | `Themes/Paleta.axaml`: `ThemeDictionaries` Light/Dark amb 23 pinzells (`InfoBrush`, `DangerContainerBrush`…). Cap `#RRGGBB` a cap altre fitxer (R6) |
| **Convenció CRUD** | `X{Create,Update,Set}Window` + `XRowUserCtrl`, per a 6 entitats |

### Inventari de vistes

**22 `Window`**:
`MainWindow` · `{Actuacio,Alumne,Centre,CursAcademic,Etapa,TipusActuacio}CreateWindow` ·
`{…}UpdateWindow` (6) · `{…}SetWindow` (6) · `UtilitatsWindow` · `AlumneInformeViewerWindow` ·
`ConfirmacioWindow` (R6)

**9 `UserControl`**: `{Actuacio,Alumne,Centre,CursAcademic,Etapa,TipusActuacio}RowUserCtrl`
*(instanciats pel `ListBox.ItemTemplate`, amb `DataContext` heretat de l'`ItemsSource` — el contenidor no els construeix; obtenen la factory pel constructor pont, veure §R0.5)* ·
`DateInput` · `LookupInput` · `IndicadorCarrega` (R6) *(controls de presentació, sense ViewModel)*

### Signatures dels ViewModels (determinen què pot fer la factory)

Des de R1 tots porten l'`IServiceFactory` com a **primer** paràmetre — l'ordre el vigila
`FabricaDeServeisTest` (§R1.6). `serveis` s'abrevia a `IServiceFactory` a la taula.

| Grup | Signatura | Resoluble per DI sense args? | Qui el construeix |
|---|---|---|---|
| `{Alumne,Centre,CursAcademic,Etapa,TipusActuacio}CreateViewModel` | `(IServiceFactory)` | ✅ Sí | el `*SetViewModel` pare, via `Interaction` |
| `AppStatusViewModel`, `UtilitatsViewModel` | `(IServiceFactory)` | ✅ Sí | `Get<T>()` |
| `ConfirmacioViewModel` | `(string titol = …, string missatge = …, string textAfirmatiu = …)` | ✅ Sí (tot amb default) | `RegistraConfirmacio` — **no parla amb el BL**, i per això no rep la fàbrica |
| `ActuacioCreateViewModel` | `(IServiceFactory, int? alumneId = null)` | ✅ Sí | `ActuacioSetViewModel`, via `Interaction` |
| `{Alumne,Centre,CursAcademic,Etapa,TipusActuacio}SetViewModel` | `(IServiceFactory, bool modeLookup = false)` | ✅ Sí | `Get<T>()` des del menú, `Get<T>(modeLookup)` als 16 lookups |
| `ActuacioSetViewModel` | `(IServiceFactory, bool modeLookup = false, int? alumneId = null)` | ✅ Sí | igual, i `AlumneRowViewModel` amb `alumneId` |
| `{…}UpdateViewModel` (6) | `(IServiceFactory, int id)` | ❌ Requereix arg de runtime | el `*RowViewModel` pare, via `Interaction` |
| `AlumneInformeViewerViewModel` | `(IServiceFactory, int alumneId)` | ❌ Requereix arg de runtime | `Alumne`/`ActuacioRowViewModel`, via `Interaction` |
| `{…}RowViewModel` (6) | `(IServiceFactory, DTO data, …, bool modeLookup)` | ❌ Requereix arg de runtime | el `*SetViewModel` pare, en bucle |

---

## 2. Invariants — no trencar

1. **Els `Window` d'Avalonia no es poden reobrir després de `Close()`.** Qualsevol registre al contenidor ha de ser **`Transient`**. Un `Singleton` peta al segon `ShowDialog`.
2. **`ReactiveWindow<T>` sincronitza `DataContext` ↔ `ViewModel`**. Assignar `DataContext` és suficient; el codi actual ja hi confia (`.WhenAnyValue(x => x.ViewModel)`).
3. **Els `*SetViewModel` disparen la càrrega de dades *dins* del constructor**:
   ```csharp
   this.WhenAnyValue(x => x.NomesActius).Subscribe(nomesActius => LoadCentres(nomesActius));
   ```
   `WhenAnyValue` emet immediatament → `LoadX()` s'executa durant la construcció i ja llegeix `ModeLookup`.
   ⚠️ **`ModeLookup` NO es pot convertir en `{ get; init; }` assignat després del constructor.** Ha de continuar sent argument del constructor.
4. **A l'AXAML no hi ha `Design.DataContext`: hi ha `x:DataType`.** Els 14 blocs que hi havia instanciaven el ViewModel, i per tant li exigien constructor sense paràmetres — incompatible amb la injecció de R1. `x:DataType` només en diu el *tipus*. Va acompanyat de `x:CompileBindings="True"`, i això vol dir que **cada `{Binding}` d'aquells 14 fitxers es comprova en compilar**: un nom de propietat mal escrit ja no és un binding buit en silenci, és un `AVLN2000`. Compte amb dues coses: el `clr-namespace` dels ViewModels **necessita el `;assembly=UI.ER.ViewModels`** (sense ell `x:DataType` no resol, tot i que `Design.DataContext` sí que ho feia), i dins d'un `DataTemplate` el tipus l'infereix de l'`ItemsSource`, així que els bindings de les plantilles també es comproven.
5. **Cap ViewModel resol serveis pel seu compte.** Reben `IServiceFactory` pel constructor i el guarden a `_serveis`; les operacions de BL surten sempre de `using var bl = _serveis.GetBLOperation<IXxx>()`. La fàbrica és **`AddScoped`**: resolta des del provider arrel —el que feia `SuperContext`— les operacions transitòries `IDisposable` s'acumularien fins a tancar l'aplicació. Ho vigila `FabricaDeServeisTest` (§R1.6). Conseqüència al registre: el filtre de `DI/Injection.cs` demana que **tots els paràmetres del constructor siguin resolubles** —registrats o amb valor per defecte—, no que tots tinguin valor per defecte; i per això l'`IServiceFactory` s'ha de registrar **abans** de l'escaneig dels ViewModels.
6. **`UI.ER.AvaloniaUI.Test` cobreix les invariants estructurals de R0, R1, R3 i R4** (convenció, registre, cicles de vida, constructors, herència de les classes base, navegació per comanda, injecció de la fàbrica de serveis). No cobreix res que necessiti una plataforma d'Avalonia ni la base de dades: això es valida amb `dotnet build` + prova manual. Veure §R0.9, §R1.6 i §R3.5.
7. **Tota vista ha de conservar un constructor `public` sense paràmetres.** Els `*RowUserCtrl` perquè els instancia el `ListBox.ItemTemplate`; les finestres perquè, si no, el compilador d'Avalonia emet `AVLN3001`. Les que necessiten serveis fan servir el constructor pont encadenat sobre `App.Services` (§R0.5).
8. **Les vistes ja no fan `new` d'altres vistes.** Tot passa per `IWindowFactory`. La validació d'arrencada de `DI/Injection.cs` peta si s'afegeix una finestra fora de convenció sense `[ViewModel(typeof(...))]`.
9. **Cada vista amb `x:Class` conserva el seu `InitializeComponent()`.** `AvaloniaXamlLoader.Load(this)` es queda a la classe derivada, mai a la classe base: el compilador d'Avalonia el reescriu a una crida directa al mètode generat només quan el troba dins del tipus que declara l'AXAML. Pujar-lo a la base compilaria igual però passaria a resoldre's per reflexió en temps d'execució.
10. **Els escanejos de `DI/Injection.cs` i de `Vistes.cs` filtren `IsAbstract` i `IsGenericTypeDefinition`.** És el que manté les classes base de R3 fora del registre i fora dels tests d'inventari. No treure aquests filtres.
11. **`RequestedThemeVariant` (App.axaml) i `BaseTheme` (`MaterialTheme`) han d'anar sempre iguals.** El primer tria quina taula de `Themes/Paleta.axaml` s'aplica; el segon, la del `MaterialTheme`. Si es deixa `RequestedThemeVariant` sense fixar, Avalonia segueix el tema del sistema operatiu i la paleta pròpia se'n va a fosc mentre Material es queda clar. Canviar de tema és tocar-los tots dos.
12. **Cap color s'escriu a pèl.** Tot surt d'una clau de `Themes/Paleta.axaml` o del `MaterialTheme`. Ho vigila `DissenyTest` (§R6.4), que escaneja el *codi font* — un literal compila igual de bé que una clau, i el que es vol vigilar és què s'escriu.
13. **Cap vista navega des d'un handler de `Click`.** Cada entrada de menú i cada botó que obre una finestra és una `ICommand` del ViewModel amb la seva `Interaction`; la vista només diu quina finestra l'atén (`RegistraNavegacio<TWindow>`). Ho vigila `NavegacioTest` (§R4.3). Els handlers que queden a `MainWindow` no naveguen: escriuen a la snackbar o mouen el `Carousel`.

---

## R0 — `IWindowFactory` + registre per comprensió ✅ FET

> Estat: implementat i compilant net. Aquesta secció descriu **el que hi ha al codi**,
> no una proposta. És la referència d'API per a R1, R3 i R4.

### R0.1 — Què hi ha ara

| Fitxer | Contingut |
|---|---|
| `UI.ER.AvaloniaUI/Services/IWindowFactory.cs` | `Get<TWindow>()` i `GetWith<TWindow>(ViewModelBase dataContext)` |
| `UI.ER.AvaloniaUI/Services/WindowFactory.cs` | Implementació + `public static Type ViewModelTypeFor(Type viewType)` |
| `UI.ER.AvaloniaUI/Services/ViewModelAttribute.cs` | `[ViewModel(typeof(...))]`, excepció declarativa a la convenció |
| `UI.ER.AvaloniaUI/DI/Injection.cs` | `UIConfigureServices()`: escaneig de vistes i ViewModels + validació d'arrencada |
| `UI.ER.AvaloniaUI/App.axaml.cs` | Composition root; exposa `App.Services`; `desktop.MainWindow = factory.Get<MainWindow>()` |

Composition root actual (R1 hi ha esborrat les dues línies de `SuperContext`; l'`IServiceFactory`
ara el registra `UIConfigureServices()` — §R1.2):

```csharp
_services = new ServiceCollection()
    .DataLayerConfigureServices()
    .BusinessLayerConfigureServices()
    .UIConfigureServices()
    .BuildServiceProvider();

desktop.MainWindow = _services.GetRequiredService<IWindowFactory>().Get<MainWindow>();
```

`UIConfigureServices()` registra **per comprensió**, sense cap llista escrita a mà:

- **Vistes**: tot tipus no abstracte assignable a `Window` de l'assembly de la UI → `AddTransient`.
  21 finestres. `Transient` és obligatori (invariant 1).
- **ViewModels**: tot tipus no abstracte assignable a `ViewModelBase` **que tingui un
  constructor amb tots els paràmetres amb valor per defecte** → `AddTransient`. En surten 15.
  El filtre exclou automàticament els 6 `{…}UpdateViewModel(int id)`, els 6 `{…}RowViewModel`
  i l'`AlumneInformeViewerViewModel(int alumneId)`: aquests **sempre** passen per `GetWith`.
- **`IWindowFactory` → `WindowFactory`** com a `Singleton` (no té estat propi; obre un scope per crida).

### R0.2 — Convenció Vista → ViewModel

Treure el sufix `Window` o `UserCtrl`, afegir `ViewModel`, buscar-lo a
`typeof(ViewModelBase).Namespace` de l'assembly `UI.ER.ViewModels`.

L'única excepció és `MainWindow`, marcada amb `[ViewModel(typeof(AppStatusViewModel))]`.
L'atribut té prioritat sobre la convenció; no hi ha cap `switch` per casos especials.

**Validació d'arrencada** (`Injection.ValidaConvencioVistaViewModel`): a `UIConfigureServices()`
s'itera **cada** finestra de l'assembly i es comprova que `WindowFactory.ViewModelTypeFor`
resol. Si alguna falla, es llança amb la llista completa de vistes problemàtiques.
Afegir una finestra fora de convenció i sense atribut peta a l'arrencada, no en obrir el diàleg.

> Substitueix el «test d'arrencada» del pla original: la solució no té projecte de tests d'UI,
> i una comprovació al composition root dona la mateixa garantia de fallada ràpida.
> Verificat en un arnès temporal: les 21 finestres resolen, i les 15 registrades ho són com a `Transient`.
>
> La convenció es compleix **també als 32 punts de crida de `GetWith`**. Comprovat de forma
> mecànica: canviant temporalment la signatura a `GetWith<TWindow, TVm>` i generant el segon
> argument de tipus *a partir de la convenció*, el projecte compila sense un sol error — és a dir,
> el que es passa a cada punt de crida és assignable al ViewModel que la convenció prediu.

### R0.3 — Scope per diàleg

```csharp
private TWindow Build<TWindow>(Func<IServiceScope, object> dataContextFactory) where TWindow : Window
{
    var scope = provider.CreateScope();
    try
    {
        var window = scope.ServiceProvider.GetRequiredService<TWindow>();
        window.DataContext = dataContextFactory(scope);
        window.Closed += (_, _) => scope.Dispose();
        return window;
    }
    catch { scope.Dispose(); throw; }
}
```

⚠️ **A R0 això no arreglava res encara**: els ViewModels demanaven els serveis BL amb
`SuperContext.Resolve<T>()`, que resolia **al provider arrel**, fora de l'scope del diàleg.

✅ **Des de R1 l'scope té efecte de debò.** Els ViewModels reben una `IServiceFactory`
`AddScoped` pel constructor, i per tant les operacions de BL que creen queden apuntades a
l'scope del diàleg (§R1.2). Amb dos matisos:
- Amb `Get<T>(vmArgs)` funciona sol: el VM es resol de `scope.ServiceProvider`.
- Amb `GetWith<T>(dc)` el VM arriba construït des de fora, i fa servir la fàbrica de l'scope
  de **qui l'ha construït** — el ViewModel pare. Les seves operacions s'alliberen quan es
  tanca la finestra pare, no la seva. Veure §R1.4.

### R0.4 — API i quan fer servir cada operació

L'API de R0 era aquesta:

```csharp
TWindow Get<TWindow>()                                where TWindow : Window;
TWindow GetWith<TWindow>(ViewModelBase dataContext)   where TWindow : Window;
```

```csharp
// Sense arguments de runtime: la factory resol vista i ViewModel.
var w = _windows.Get<CentreSetWindow>();

// Amb ViewModel ja construït: cas `interaction.Input` i cas dels args de runtime.
var w = _windows.GetWith<CentreUpdateWindow>(interaction.Input);
var w = _windows.GetWith<AlumneSetWindow>(new AlumneSetViewModel(modeLookup: true));
```

`Get<T>()` només funcionava si el ViewModel estava registrat, és a dir si tots els paràmetres
del seu constructor tenien valor per defecte. Per a la resta, `GetWith`.

**R1 ha eixamplat la primera** a `Get<TWindow>(params object[] vmArgs)`, i amb això el tercer
exemple ha passat a `_windows.Get<AlumneSetWindow>(modeLookup)`, sense cap `new`. El segon no
ha canviat: veure §R1.3 i §R1.4.

**`GetWith` no difereix de `Get` en el *tipus* de ViewModel, només en *qui el construeix*.**
Per això el paràmetre és `ViewModelBase` (no `object`) i, a més, `WindowFactory.VerificaParella`
comprova que sigui una instància del ViewModel que la convenció assigna a `TWindow`;
si no, llança abans d'obrir el diàleg. Sense la comprovació, un desaparellament es manifestaria
com un diàleg amb els bindings buits i cap error.

> **Per què no la forma totalment tipada**
> `GetWith<TWindow, TVm>(TVm vm) where TWindow : ReactiveWindow<TVm>` comprovaria la parella en
> temps de compilació, però **C# no fa inferència parcial de tipus genèrics**: o s'escriuen tots
> els arguments de tipus o cap. Els 32 punts de crida passarien a
> `GetWith<CentreUpdateWindow, CentreUpdateViewModel>(…)`, repetint el nom de l'entitat dues
> vegades. I `AlumneInformeViewerWindow` no hi encaixaria fins a resoldre **B4** (avui és un
> `Window` pelat, no un `ReactiveWindow<T>`).
> Val la pena reconsiderar-ho quan R3 tingui les classes base genèriques: allà la parella
> vista↔ViewModel ja queda fixada pel tipus base i la verificació en runtime esdevé redundant.

> **Per què no `Get<T>(params object[] args)` a R0**: `ActivatorUtilities.CreateInstance` fa el
> matching per tipus, és posicional i és opac. La reserva sobre els `int?`
> (`ActuacioSetViewModel(bool, int?)`, `ActuacioCreateViewModel(int?)`) **ha resultat infundada**
> — R1 ho ha comprovat i hi ha deixat un test (§R1.3). La resta de la crítica es manté, i per
> això `Get(vmArgs)` només s'usa on l'argument és un de sol i té nom al punt de crida.

### R0.5 — Com les vistes obtenen la factory

**17 vistes** obren diàlegs i per tant necessiten la factory: 11 finestres
(`MainWindow`, els 6 `*SetWindow`, `Actuacio{Create,Update}Window`, `Alumne{Create,Update}Window`)
i els 6 `*RowUserCtrl`. Totes segueixen **el mateix patró de dos constructors**:

```csharp
private readonly IWindowFactory _windows;

// Constructor pont: el manté el carregador XAML en temps d'execució i el
// previsualitzador d'Avalonia, que instancien la vista sense passar pel
// contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
public ActuacioCreateWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

public ActuacioCreateWindow(IWindowFactory windows)
{
    _windows = windows;
    InitializeComponent();
    …
}
```

**Per què el constructor pont també a les finestres.** Es va provar de deixar-hi només el
constructor de DI. Compila i funciona, però el compilador d'Avalonia emet
`AVLN3001: XAML resource … won't be reachable via runtime loader, as no public constructor was found`
per a cadascuna de les 11 — el criteri de «build net» ho descarta. A més, encadenar els dos
constructors fa que **no importi quin dels dos triï el contenidor**: `_windows` queda assignat
per les dues vies.

Els `*RowUserCtrl` necessiten el pont per una raó diferent i inevitable: els instancia el
`ListBox.ItemTemplate` des de l'AXAML, no el contenidor.

`App.Services` és el provider arrel, exposat com a `public static` **exclusivament** per a aquests
constructors pont. És l'únic localitzador de serveis que queda a la capa de vistes, i és
deliberat mentre l'AXAML pugui instanciar vistes pel seu compte.
⚠️ **R3 no l'ha pogut reduir, i no el reduirà mai cap classe base**: els constructors no
s'hereten en C#, i l'invariant 6 obliga cada vista a tenir el seu constructor sense
paràmetres. Continuen sent 16 punts de crida, ara d'una sola línia cadascun. L'única manera
de fer-los desaparèixer és que l'AXAML deixi d'instanciar vistes pel seu compte.

### R0.6 — Blocadors: estat final

| # | Blocador | Estat |
|---|---|---|
| **B1** | `this.DataContext = new XCreateViewModel();` al constructor d'`Actuacio/AlumneCreateWindow` | ✅ **Eliminat.** Construïa un VM que disparava les seves subscripcions i que l'`object initializer` llençava tot seguit. |
| **B2** | `new MainWindow()` a `App.axaml.cs` + `DataContext = new AppStatusViewModel()` | ✅ **Resolt** amb `factory.Get<MainWindow>()` + `[ViewModel(typeof(AppStatusViewModel))]`. |
| **B3** | 13 blocs `<Design.DataContext>` (14 amb el que hi va afegir R6) | ✅ **Resolt a R1** amb `x:DataType` + `x:CompileBindings="True"` — §R1.5. |
| **B4** | `AlumneInformeViewerWindow` és `Window` pelat, no `ReactiveWindow<T>` | ⏭️ **Sense canvis.** Funciona amb la factory (fa servir `DataContext`). Unificar-lo a R2. |
| **B5** | Lifetime | ✅ **`AddTransient` per a totes les vistes**, verificat. |
| **B6** | `SuperContext.Resolve<T>()` | ✅ **Resolt a R1.** Les 51 crides passen per una `IServiceFactory` injectada; `SuperContext.cs` esborrat. |

> ⚠️ **Actualització de R1**: `Get<TWindow>()` ha passat a ser
> `Get<TWindow>(params object[] vmArgs)`. Sense arguments es comporta exactament igual
> que abans (el ViewModel surt del contenidor); amb arguments els combina amb les
> dependències registrades. Veure §R1.3.

### R0.7 — Criteris d'acceptació

- [x] Cap `new XWindow(` als fitxers `.cs` de `UI.ER.AvaloniaUI` (43 punts de crida eliminats). L'excepció que quedava —el `new Window` anònim de `Helpers/ConfirmationDialog.cs`— ha desaparegut a **R6**: ara és `Pages/ConfirmacioWindow.axaml`, resolta per la factory.
- [x] Cap registre de vista escrit a mà a `App.axaml.cs` — tot per escaneig a `DI/Injection.cs`.
- [x] Validació d'arrencada que itera totes les finestres i comprova que `ViewModelTypeFor` resol.
- [x] **Tests de regressió**: `UI.ER.AvaloniaUI.Test`, 19 tests verds, verificats per mutació (§R0.9).
- [x] Cada diàleg allibera el seu `IServiceScope` a `Closed` *(a R0 era només cablejat; té efecte des de R1, veure R0.3)*.
- [x] `dotnet build eaprecull.sln --no-incremental` net: **0 errors i cap warning d'Avalonia, de C# ni dels analitzadors**. Els 100 warnings són tots `NU1903` de vulnerabilitats de paquets (82 abans de R0; els 18 nous els aporta el projecte de tests, que arrossega els mateixos paquets).
- [x] L'aplicació arrenca sense excepcions (`error.log` buit).
- [ ] **Prova manual pendent de l'usuari**: obrir i tancar el mateix diàleg 3 vegades seguides (valida B5 i el disposal d'scope), i recórrer els lookups d'`Actuacio{Create,Update}` i `Alumne{Create,Update}`.

### R0.9 — Tests de regressió (`UI.ER.AvaloniaUI.Test`)

Projecte xUnit nou, amb els mateixos paquets que `BusinessLayer.Integration.Test`.
**19 tests a R0; 40 després de R3, R4, R6 i R1. ~60 ms, sense base de dades i sense
plataforma gràfica.**

> ⚠️ `DataLayerConfigureServices()` **executa les migracions** com a efecte secundari del
> registre. Els tests només criden `UIConfigureServices()`, que no toca ni disc ni BD.

| Fitxer | Què fixa |
|---|---|
| `Vistes.cs` | Punt únic des d'on s'enumeren vistes i ViewModels reals. **Cap test escriu llistes de tipus a mà**: una vista nova entra sola a tots els tests. |
| `ConvencioVistaViewModelTest.cs` | Cada finestra resol el seu ViewModel; l'atribut guanya a la convenció; el missatge d'error diu què s'esperava i com arreglar-ho; `UIConfigureServices()` no llança. |
| `RegistreDITest.cs` | Totes les finestres registrades i `Transient`; es registren **exactament** els ViewModels construïbles sense arguments; els d'arguments de runtime **no** hi són; les dependències de cada constructor de finestra existeixen; `IWindowFactory` és `Singleton`. |
| `ConstructorsDeVistaTest.cs` | Tota vista té constructor públic sense paràmetres (AVLN3001 + `ItemTemplate`); **el constructor pont encadena de debò amb el de DI** (inspecció de l'IL: busca el `call` al token de l'altre constructor); tota vista amb un camp `IWindowFactory` té el constructor que el rep. |
| `WindowFactoryTest.cs` | `GetWith` rebutja un ViewModel que no és el de la finestra, i ho fa **abans** de construir res. R1 hi ha afegit `CreaViewModel`: sense arguments surt del contenidor, amb arguments els barreja amb les dependències de l'scope, i els paràmetres nullables s'omplen igual (§R1.3). |
| `FabricaDeServeisTest.cs` | **R1** — l'`IServiceFactory` és `Scoped`, no es pot resoldre fora d'un scope, es registra abans de l'escaneig dels ViewModels, cap ViewModel torna a guardar serveis en un camp estàtic, i els que porten arguments de runtime reben la fàbrica com a primer paràmetre. |

**Verificat per mutació** — no n'hi ha prou que passin, han de fallar quan toca:

| Mutació | Resultat |
|---|---|
| Treure `[ViewModel(typeof(AppStatusViewModel))]` de `MainWindow` | ❌ 13 tests |
| Afegir `InformesWindow` sense `InformesViewModel` | ❌ 13 tests, amb el missatge exacte de què falta |
| `AddSingleton(view)` en comptes d'`AddTransient` | ❌ 1 test (`TotesLesFinestresSonTransient`) |
| Constructor pont que no encadena (`public EtapaRowUserCtrl() { }`) | ❌ 1 test (`ElConstructorPontEncadenaAmbElDeDI`) |
| Treure el constructor pont de `CentreSetWindow` | ❌ 2 tests |

**Què NO cobreixen, i per què:**
- El camí feliç de `Get`/`GetWith`: construir un `Window` demana una plataforma d'Avalonia
  inicialitzada, i construir qualsevol ViewModel real dispara la càrrega de dades contra la
  BD des del constructor (invariant 3). Caldria `Avalonia.Headless` **i** una BD de proves.
- El disposal de l'`IServiceScope` en tancar el diàleg: necessita una finestra de veritat.
- La parella vista↔ViewModel als punts de crida: és una propietat del codi font, verificada
  pel compilador (§R0.2), no per un test.

### R0.8 — Deutes que R0 deixava oberts

1. ~~`GetWindow() => (Window)this.VisualRoot!` duplicat a 10 fitxers~~ → ✅ **R3**: `Helpers/VisualRootExtensions.GetOwnerWindow()`. Nom final `VisualRootExtensions`, no `TopLevelExtensions`, i el mètode fa servir `visual.GetVisualRoot()` perquè `Visual.VisualRoot` és `protected` i no es pot llegir des d'una extensió.
2. ~~Els 16 punts de lookup fent `new XSetViewModel(modeLookup: true)` a mà~~ → ✅ **R3**: `RegistraLookup<TSetWindow>`, una línia per lookup; ✅ **R1**: sense el `Func<ViewModelBase>`, el `new` ha desaparegut del tot.
3. `App.Services` als 16 constructors pont → ❌ **no reduïble**, veure §R0.5.
4. ~~La signatura `Get<TWindow>(params object[] vmArgs)`~~ → ✅ **R1**. No substitueix `GetWith`, hi conviu: veure §R1.3.

---

## R1 — Eliminar el Service Locator estàtic `SuperContext` ✅ FET

> Estat: implementat, compilant net i amb 40 tests verds. Aquesta secció descriu **el que
> hi ha al codi**, no una proposta.

### R1.1 — Resultat

`SuperContext` era un `IServiceProvider` estàtic amb un `Resolve<T>()` que els ViewModels
cridaven **51 vegades des de 26 fitxers**. Ja no existeix: `Services/SuperContext.cs` està
esborrat i cap ViewModel es pot construir sense que algú li doni les seves dependències.

| | Abans | Ara |
|---|---|---|
| Com arriba un VM al BusinessLayer | `SuperContext.Resolve<ICentreCreate>()` | `_serveis.GetBLOperation<ICentreCreate>()` |
| D'on surt `_serveis` | enlloc: era estàtic | `IServiceFactory`, primer paràmetre del constructor |
| Registre de la fàbrica | `AddSingleton<IServiceFactory, SuperContext>` a `App.axaml.cs`, **sense injectar-se enlloc** | `AddScoped<IServiceFactory, ServiceFactory>` a `DI/Injection.cs` |
| D'on surten les operacions de BL | provider **arrel** → vives fins a tancar l'app | scope **del diàleg** → alliberades en tancar-lo |
| VMs amb constructor injectat | 0 | **27** |
| Crides a `SuperContext` | 51 | **0** |
| `new XViewModel(...)` al codi de les *vistes* | 16 (`Func<ViewModelBase>` dels lookups) + 1 (confirmació) | **1** (`ConfirmacioViewModel`, que no parla amb el BL) |
| Blocs `<Design.DataContext>` | 14 | **0** — `x:DataType` + `x:CompileBindings="True"` |

Fitxers nous: `UI.ER.ViewModels/Services/ServiceFactory.cs` (35 línies, gairebé tot comentari)
i `UI.ER.AvaloniaUI.Test/FabricaDeServeisTest.cs`.

### R1.2 — Per què una fàbrica i no els serveis concrets

Els ViewModels **no** poden rebre `ICentreCreate` i companyia pel constructor: les operacions
de BL són `AddTransient` i `IBLOperation : IDisposable`, i els VMs les consumeixen amb
`using var bl = …` a **cada** crida. Una instància injectada quedaria disposada després del
primer ús i la segona obertura del diàleg petaria. Cal una fàbrica: `IServiceFactory` (o un
`Func<IXxx>` per servei, que és el mateix amb més soroll).

`IServiceFactory` **no és un `IServiceProvider` disfressat**: la seva única operació és
`T GetBLOperation<T>() where T : IBLOperation`, i des d'aquí no s'arriba a cap altre servei
del contenidor.

**`AddScoped` no és cosmètic.** És el que fa que l'scope per diàleg de la `IWindowFactory`
(§R0.3) serveixi de debò: MS.DI apunta els transitoris `IDisposable` a l'scope que els ha
creat, per disposar-los quan es tanca. `SuperContext` resolia des de l'**arrel**, i per tant
cada crida afegia l'operació a una llista que només es buidava en tancar l'aplicació. El
`using var bl = …` **sí** que n'alliberava el `DbContext` (`BLOperation.Dispose()` el disposa
i el posa a `null`), i per tant no era una fuita de connexions; el que creixia sense aturador
era la llista de disposables de l'arrel, amb un objecte per cada operació que s'hagués fet mai.
Ara la llista és la de l'scope del diàleg i mor amb ell. Dues coses ho vigilen:
`FabricaDeServeisTest.LaFabricaDeServeisNomesSurtDUnScope`, que comprova que
`BuildServiceProvider(validateScopes: true)` **es nega** a resoldre-la des de l'arrel, i
`LaFabricaDeServeisEsScoped`, que fixa el cicle de vida.

### R1.3 — `Get<TWindow>(params object[] vmArgs)`

`IWindowFactory.Get<TWindow>()` ha passat a acceptar arguments de runtime:

```csharp
TWindow Get<TWindow>(params object[] vmArgs) where TWindow : Window;
```

Sense arguments es comporta **exactament** com abans, i per això els set punts de navegació
de R4 i l'arrencada de `MainWindow` no han canviat ni una línia. Amb arguments, la
construcció passa per `ActivatorUtilities.CreateInstance`, que barreja els arguments donats
amb el que el contenidor sap resoldre — l'`IServiceFactory` de l'scope, en tots els casos
d'avui.

La lògica viu en un mètode `public static` a part, `WindowFactory.CreaViewModel(provider,
viewModelType, vmArgs)`, precisament perquè els tests hi arribin: és l'única part de la
factory que no necessita una plataforma d'Avalonia inicialitzada.

Dues coses que el pla original donava per certes i **no ho són**:

- «*Compte amb els `int?`: cal treure'ls dels constructors abans*». No cal.
  `ActivatorUtilities` omple els paràmetres nullables tant si l'argument ve tipat (`(int?)7`)
  com si ve pelat (`7`). Hi ha un test que ho fixa
  (`CreaViewModelOmpleElsParametresNullables`), perquè és una garantia de la implementació de
  `Microsoft.Extensions.DependencyInjection`, no del llenguatge.
- «*la signatura que substituirà els `GetWith`*». No els substitueix — veure §R1.4.

L'aparellament és **per tipus**, no per posició: dos paràmetres del mateix tipus no es poden
distingir. És per això que la `ConfirmacioWindow` —tres `string` seguits— continua passant
per `GetWith` amb un `new ConfirmacioViewModel(...)` explícit.

### R1.4 — Els diàlegs d'edició continuen amb `GetWith`, i per què

El pla proposava dues sortides per als `GetWith<XUpdateWindow>(interaction.Input)`: injectar
una `Func<int, XUpdateViewModel>` al VM pare, o fer que la `Interaction` portés l'`id` en
comptes del ViewModel sencer. **No s'ha fet cap de les dues.** Amb la fàbrica injectada, el
VM pare ja té tot el que li cal per construir el fill:

```csharp
// CentreRowViewModel
var update = new CentreUpdateViewModel(_serveis, Id);
var data = await ShowUpdateDialog.Handle(update);
```

El motiu és d'abast: la segona opció obliga a canviar les tres interfícies de
`Contracts/DialegContracts.cs` i les tres classes base de R3, i R1 ja toca 58 fitxers. La
primera afegeix 6 delegats al contenidor per estalviar 6 `new` que ara són trivials.

**El que això deixa obert**, i que convé saber: el VM del diàleg d'edició fa servir la fàbrica
de l'scope del **pare**. Obrir i tancar la fitxa d'un centre 20 vegades acumula les seves
operacions de BL a l'scope de la `CentreSetWindow`, no a la de la finestra d'edició, i
s'alliberen quan es tanca la llista. Segueix sent una millora estricta sobre el provider
arrel —abans era «fins a tancar l'aplicació»—, però no és el comportament ideal. Qui vulgui
tancar-ho ha de fer que la `Interaction` porti l'`id`.

### R1.5 — B3: `Design.DataContext` → `x:DataType` + compiled bindings

Els 14 blocs

```xml
<Design.DataContext>
    <viewModels:CentreSetViewModel />
</Design.DataContext>
```

**instanciaven** el ViewModel, i per tant li exigien constructor sense paràmetres. Han passat
a dos atributs a l'element arrel:

```xml
x:DataType="viewModels:CentreSetViewModel"
x:CompileBindings="True"
```

Dues troballes que costa endevinar:

1. **El `clr-namespace` necessitava el `;assembly=`.** 13 dels 14 fitxers declaraven
   `xmlns:viewModels="clr-namespace:UI.ER.ViewModels.ViewModels"` sense assembly. Amb
   `<Design.DataContext>` funcionava; amb `x:DataType` no, i el compilador escup
   `AVLN2000: Unable to resolve type`. Ara tots porten
   `;assembly=UI.ER.ViewModels`.
2. **La primera compilació neta és enganyosa.** `dotnet build` incremental no recompila
   l'AXAML: els 13 `AVLN2000` no van sortir fins a fer `--no-incremental`. Qualsevol canvi a
   l'AXAML s'ha de verificar amb `--no-incremental`.

**Els compiled bindings són reals i estan verificats per mutació**: canviar
`{Binding NomesActius}` per `{Binding NomesActiusXX}` a `CentreSetWindow.axaml` dona
`AVLN2000: Unable to resolve property or method of name 'NomesActiusXX' on type
'CentreSetViewModel'`. I dins d'un `DataTemplate` sense `x:DataType` propi el tipus
**s'infereix de l'`ItemsSource`**: trencar `{Binding DataTxt}` dins de l'`ItemTemplate`
d'`AlumneInformeViewerWindow` dona l'error sobre `DTO.o.DTOs.ActuacioInformeItem`. Tots els
bindings d'aquells 14 fitxers, plantilles incloses, quadren tal com estaven.

El `{Binding}` sense camí (els `ItemsControl` de `BrokenRules`, que iteren `string`) no
declara cap propietat i per tant compila sense necessitar `x:DataType` a la plantilla.

### R1.6 — Tests (`UI.ER.AvaloniaUI.Test`, 32 → 40)

`FabricaDeServeisTest.cs` (5 tests) i tres tests nous a `WindowFactoryTest.cs`.

| Test | Què fixa |
|---|---|
| `LaFabricaDeServeisEsScoped` | Cicle de vida i implementació: `Scoped` + `ServiceFactory`. |
| `LaFabricaDeServeisNomesSurtDUnScope` | Amb `validateScopes: true` l'arrel **es nega** a resoldre-la; des d'un scope, sí. És l'invariant de §R1.2 escrit com a test. |
| `LaFabricaDeServeisEsRegistraAbansDelsViewModels` | Ordre dins de `UIConfigureServices`. Si es registrés després, el filtre no en sabria res i **no es registraria cap ViewModel**. |
| `CapViewModelGuardaElsServeisEnUnCampEstatic` | Escaneja tot l'assembly de ViewModels buscant camps estàtics d'`IServiceProvider` o d'`IServiceFactory`. És la guarda contra el retorn de `SuperContext` sota un altre nom. |
| `ElsViewModelsAmbArgumentsDeRuntimeReepLaFabricaPelConstructor` | Els que el contenidor no pot construir (Update, Row, InformeViewer) reben la fàbrica com a **primer** paràmetre; és el que permet que el VM pare els la passi. |
| `CreaViewModelSenseArgumentsElTreuDelContenidor` | El camí de sempre no ha canviat. |
| `CreaViewModelBarrejaElsArgumentsDeRuntimeAmbLesDependencies` | El cas dels 16 lookups: la vista només diu `modeLookup`, la fàbrica de serveis la posa l'scope. |
| `CreaViewModelOmpleElsParametresNullables` | La trampa dels `int?` que el pla anunciava i que no s'ha materialitzat (§R1.3). |

Els dos primers fan servir un `FakeBL : IServiceFactory` i un ViewModel de mentida amb la
mateixa forma de constructor que `ActuacioSetViewModel` — que és exactament el
`FakeBL` amb un diccionari que el pla anunciava com a benefici de R1: **provar un ViewModel
ja no demana estat global**.

**Verificat per mutació:**

| Mutació | Resultat |
|---|---|
| `AddScoped` → `AddSingleton` per a `IServiceFactory` | ❌ 2 tests (`…EsScoped`, `…NomesSurtDUnScope`) |
| Registrar la fàbrica **després** de l'escaneig dels ViewModels | ❌ 3 tests, i el missatge diu que no s'ha registrat cap ViewModel |
| `private static IServiceFactory? _global;` a `AppStatusViewModel` | ❌ 1 test (`CapViewModelGuardaElsServeis…`) |
| Moure l'`IServiceFactory` al segon lloc del constructor d'`EtapaRowViewModel` | ❌ 1 test (`…ReepLaFabricaPelConstructor`) |
| `{Binding NomesActiusXX}` a `CentreSetWindow.axaml` | ❌ compilació (`AVLN2000`), no test — §R1.5 |

`Vistes.EsConstruiblePelContenidor` ha canviat de signatura (`(Type, IServiceCollection)`)
per continuar sent el mirall de la regla de `DI/Injection.cs`. La còpia és deliberada: si la
regla del registre canvia sense actualitzar el mirall,
`RegistreDITest.EsRegistrenExactamentElsViewModelsConstruiblesSenseArguments` es posa vermell.

### R1.7 — El que R1 **no** ha fet

1. **Els 19 fitxers d'AXAML que no tenien `<Design.DataContext>`** continuen amb bindings per
   reflexió: les 12 finestres `{Create,Update}`, `MainWindow`, `UtilitatsWindow`, `App.axaml`,
   `Themes/Paleta.axaml` i els controls `DateInput`, `LookupInput`, `IndicadorCarrega`.
   Posar-los `x:DataType` + `x:CompileBindings` és **R8**, i és on hi ha més a guanyar: les
   finestres d'edició són les que tenen més bindings i on una propietat mal escrita passa més
   desapercebuda. Els dos controls amb `StyledProperty` són un cas diferent —no tenen un
   ViewModel propi— i probablement només vulguin `x:CompileBindings` a les plantilles.
2. **`AlumneInformeViewerWindow` continua sent un `Window` pelat** (B4/R2): el seu ViewModel
   sí que rep la fàbrica, però la finestra segueix subscrivint-se dins del constructor, sense
   `WhenActivated` ni disposal, i amb `async void` a `Opened`. Convertir-lo a
   `ReactiveWindow<AlumneInformeViewerViewModel>` és un canvi d'un sol fitxer — veure §R2.
3. **El `new ConfirmacioViewModel(...)` de `RegistraConfirmacio`** es queda: aquell VM no parla
   amb el BusinessLayer i porta tres `string`, que `ActivatorUtilities` no sap distingir
   (§R1.3).
4. **Els 16 `App.Services.GetRequiredService<IWindowFactory>()` dels constructors pont**: no
   són reduïbles, §R0.5.

### R1.8 — Pendent de validació manual

- [ ] Obrir cada llista des del menú i comprovar que carrega dades (valida
      `Get<T>()` + `IServiceFactory` de l'scope).
- [ ] Recórrer els **16 lookups** d'`Actuacio{Create,Update}` i `Alumne{Create,Update}`:
      cada un ha d'obrir la llista **en mode selecció** (botó de triar visible) i tornar
      l'element. És el camí que ha canviat de `GetWith(new XSetViewModel(true))` a
      `Get<XSetWindow>(modeLookup)`.
- [ ] Obrir la fitxa d'edició d'una fila de cada entitat (valida que el VM pare passa bé la
      fàbrica al fill).
- [ ] Esborrar una actuació (valida `ShowDeleteConfirmation` + `ConfirmacioWindow`, l'únic
      `GetWith` amb `new` que queda).
- [ ] Obrir l'expedient d'un alumne i exportar-lo a Word (valida
      `AlumneInformeViewerViewModel`, que rep la fàbrica i és l'únic VM amb `DataContext`
      pelat).
- [ ] Obrir i tancar el mateix diàleg 3 vegades seguides. Ara sí que té sentit mirar-s'ho:
      des de R1 cada tancament allibera operacions de BL de debò.
- [ ] `error.log` buit. L'arrencada ja està comprovada (l'app aixeca `MainWindow` +
      `AppStatusViewModel`, que fa tres crides al BL des de l'scope, sense excepcions).

---

## R2 — Subscripcions imbricades no alliberades ✅ FET dins de R3

**Era el bug**: a totes les Create/Update/Set windows i a tots els RowUserCtrl només es
registrava la subscripció **exterior**; la interior — i cada `RegisterHandler`, que també
retorna `IDisposable` — quedava viva. En reactivar-se la vista, `CloseIfSaved` s'executava
N vegades i els handlers d'`Interaction` s'acumulaven.

**Per què s'ha fet aquí i no en un commit propi**: R3 movia justament aquestes subscripcions
a tres classes base. Escriure-les amb el bug a dins per treure'l acte seguit no tenia sentit,
i deixar-les com estaven hauria estat una **regressió** a les quatre finestres d'`Alumne`/
`Actuacio`, que sí que registraven bé la seva subscripció a `SubmitCommand` perquè no era
imbricada.

**La correcció**: `Register` rep un `CompositeDisposable` (no un `Action<IDisposable>`) i
`PerCadaViewModel` el propaga cap endins:

```csharp
protected void PerCadaViewModel(CompositeDisposable d, Action<TVm, CompositeDisposable> accio)
    => this.WhenAnyValue(x => x.ViewModel)
           .Where(vm => vm is not null)
           .Subscribe(vm => accio(vm!, d))
           .DisposeWith(d);          // ← exterior

// i cada crida registra la seva:
PerCadaViewModel(d, (vm, dd) => vm.SubmitCommand.Subscribe(TancaSiDesat).DisposeWith(dd));
```

> ⚠️ `DisposeWith` viu a `System.Reactive.Disposables.Fluent` (System.Reactive 6.1), **no** a
> `System.Reactive.Disposables`. Calen els dos `using` al mateix fitxer.

**Què queda**: `AlumneInformeViewerWindow.axaml.cs` — el cas pitjor que ja anotava R0/B4.
Subscriu dins del constructor, sense `WhenActivated` ni disposal, i amb `async void` a
`Opened`. No hereta de cap classe base perquè és un `Window` pelat, no un
`ReactiveWindow<T>`. Convertir-lo a `ReactiveWindow<AlumneInformeViewerViewModel>` i moure
les subscripcions a `WhenActivated` és un canvi d'un sol fitxer.
R5 li ha tret el `using System.Reactive.Disposables;`, que era mort precisament per això:
en fer la conversió, tornar-lo a posar (i afegir `System.Reactive.Disposables.Fluent`).

---

## R3 — Eliminar el boilerplate de diàlegs ✅ FET

### R3.1 — Resultat

| Grup | Abans | Ara |
|---|---|---|
| `{Centre,CursAcademic,Etapa,TipusActuacio}{Create,Update}Window.axaml.cs` | 8 × 45–47 línies, idèntics tret del tipus (i d'un salt de línia a `TipusActuacioUpdateWindow`) | 8 × **18** |
| `{…}SetWindow.axaml.cs` (6, amb `Alumne` i `Actuacio`) | 6 × 57–58 | 6 × **22** |
| `{…}RowUserCtrl.axaml.cs` (4 simples) | 4 × 72 | 4 × **21** |
| `AlumneRowUserCtrl` / `ActuacioRowUserCtrl` | 127 / 105 | **46** / **42** |
| `Alumne{Create,Update}Window` / `Actuacio{Create,Update}Window` | 85 / 82 / 101 / 122 | **60** / **57** / **68** / **90** |
| **Total `*.axaml.cs` del projecte** | **2.233** | **1.308** (−925) |

A canvi, **368 línies** de codi compartit nou, escrit un sol cop.

### R3.2 — Fitxers nous

| Fitxer | Contingut |
|---|---|
| `UI.ER.ViewModels/ViewModels/Contracts/DialegContracts.cs` | `ISubmitViewModel<TDto>`, `ISetViewModel<TCreateVm,TDto>`, `IRowViewModel<TUpdateVm,TResultat,TDto>` |
| `UI.ER.AvaloniaUI/Pages/Base/EntityEditWindow.cs` | `EntityEditWindow<TVm,TDto>` — tancament automàtic en desar |
| `UI.ER.AvaloniaUI/Pages/Base/EntitySetWindow.cs` | `EntitySetWindow<TVm,TCreateVm,TCreateWindow,TDto>` — diàleg d'alta |
| `UI.ER.AvaloniaUI/Pages/Base/EntityRowUserCtrl.cs` | `EntityRowUserCtrl<TVm,TUpdateVm,TUpdateWindow,TResultat,TDto>` + sobrecàrrega de 4 paràmetres per al cas `TResultat == TDto` |
| `UI.ER.AvaloniaUI/Helpers/DialegExtensions.cs` | `RegistraDialeg<…>` (amb resultat i sense) i `RegistraLookup<TSetWindow>` |
| `UI.ER.AvaloniaUI/Helpers/FileExplorer.cs` | `Obre(SaveResult?)` / `Obre(string)`, multiplataforma |
| `UI.ER.AvaloniaUI/Helpers/VisualRootExtensions.cs` | `GetOwnerWindow()` |
| `UI.ER.AvaloniaUI.Test/ClassesBaseTest.cs` | 5 tests que impedeixen que el boilerplate torni |

### R3.3 — Decisions que es desvien del pla original

1. **Una sola classe base per als diàlegs d'edició, no dues.** El pla deia
   `EntityCreateWindow<…>` i `EntityUpdateWindow<…>`; els vuit fitxers eren idèntics byte a
   byte tret del tipus, i dues classes base idèntiques haurien estat la mateixa duplicació
   una capa més amunt. És `EntityEditWindow<TVm,TDto>`, i la diferència real d'`Actuacio`
   (que també esborra) s'expressa amb un `override` de `ResultatDeTancament`.

2. **Els ViewModels han hagut d'implementar tres interfícies.** Una classe base genèrica no
   pot cridar `vm.SubmitCommand` sense una restricció que ho garanteixi. L'alternativa era
   un membre abstracte per vista (`protected abstract IObservable<TDto?> Submitted(TVm vm);`),
   que hauria tornat a posar una línia a cada fitxer. Les interfícies són **purament
   declaratives**: cap ViewModel canvia de comportament ni de signatura, i el projecte
   compila sense cap warning de nul·labilitat.

3. **R2 s'ha fet aquí.** Veure §R2.

4. **`InitializeComponent()` es queda a cada vista.** Veure invariant 8.

5. **`Process.Start` passa a ser multiplataforma.** `Verb = "open"` només s'informa a
   Windows; a macOS i Linux `UseShellExecute = true` ja delega a `open` i `xdg-open`.

6. **Les propietats `OperationResult<T> Result` (11) no s'han esborrat**: són codi mort,
   però esborrar-les és R5. Duen un comentari `// ToDo (R5)` perquè no semblin oblit.

### R3.4 — Com queda una vista

```csharp
// 18 línies en total
public partial class CentreCreateWindow : EntityEditWindow<CentreCreateViewModel, Dtoo.Centre>
{
    public CentreCreateWindow() => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}

public partial class CentreSetWindow
    : EntitySetWindow<CentreSetViewModel, CentreCreateViewModel, CentreCreateWindow, Dtoo.Centre>
{
    public CentreSetWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }
    public CentreSetWindow(IWindowFactory windows) : base(windows) => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
```

I una que hi afegeix coses, sense repetir res del que ja fa la base:

```csharp
protected override void Register(CompositeDisposable d)
{
    base.Register(d);                       // tancar en desar

    PerCadaViewModel(d, (vm, dd) =>
    {
        this.RegistraLookup<AlumneSetWindow>(_windows, vm.ShowAlumneLookup,
            () => new AlumneSetViewModel(modeLookup: true)).DisposeWith(dd);
        // …quatre lookups més
    });
}
```

### R3.5 — Tests

`ClassesBaseTest.cs`, 5 tests nous (24 en total al projecte, tots verds):

| Test | Què impedeix |
|---|---|
| `ElsDialegsDEdicioHeretenDeEntityEditWindow` | que una `*{Create,Update}Window` nova es torni a escriure el patró |
| `LesLlistesHeretenDeEntitySetWindow` | idem per a les `*SetWindow` |
| `LesFilesHeretenDeEntityRowUserCtrl` | idem per als `*RowUserCtrl` |
| `CapVistaEsFaLaSevaPropiaGetWindow` | que tornin les 10 còpies de `GetWindow()` |
| `CapVistaObreLExploradorPelSeuCompte` | que tornin les 3 còpies d'`ObraFileExplorer()` |

Els tres primers duen una llista explícita d'excepcions (`MainWindow`, `UtilitatsWindow`,
`AlumneInformeViewerWindow`) i un `Assert.NotEmpty` sobre els candidats, perquè no puguin
passar en va.

També s'ha reforçat `ConstructorsDeVistaTest.LesVistesQueObrenDialegsDemanenLaFactory`:
mirava els camps del tipus, i des de R3 qui guarda la `IWindowFactory` sol ser la classe base
(`Vistes.UsaLaFactory` ara recorre tota la jerarquia — `GetFields` no retorna els camps
privats de les classes base).

### R3.6 — Pendent de validació manual

El build compila net (0 errors, cap warning de C# ni d'Avalonia) i els 24 tests passen, però
**cap test obre una finestra**: cal una plataforma d'Avalonia i una BD (§R0.9). Cal comprovar
a mà:

- Obrir i tancar cada `*SetWindow` i donar d'alta un ítem (valida `EntitySetWindow`).
- Editar una fila des de la llista (valida `EntityRowUserCtrl` + `EntityEditWindow`).
- Els 16 lookups d'`Actuacio{Create,Update}` i `Alumne{Create,Update}`.
- Esborrar una actuació des d'`ActuacioUpdateWindow` (valida `ResultatDeTancament`).
- Seleccionar una fila en mode lookup (valida `SeleccionarCommand` → `Close`).
- Generar un informe d'alumne i el pivot d'`Utilitats` (valida `FileExplorer.Obre`).

---

## R4 — Treure la navegació del code-behind de `MainWindow` ✅ FET

> `MainWindow.axaml.cs`: **222 → 143 línies** (−79). `Helpers/DialegExtensions.cs` +39,
> `AppStatusViewModel` +17, `NavegacioTest.cs` +101. Net al projecte de la UI: **−40 línies**.
> `dotnet build` net i **27 tests verds** (24 + 3 de nous).

### R4.1 — Què hi havia i què hi ha

Hi havia **set handlers `X_OnClick` idèntics** al code-behind…

```csharp
private void Centre_OnClick(object? sender, RoutedEventArgs e)
{
    var w = _windows.Get<CentreSetWindow>();
    w.ShowDialog(this);
}
```

…i **tres `RegisterShowXDialog`** gairebé iguals per a les targetes del taulell, cadascun amb
la seva còpia de `WhenAnyValue(x => x.ViewModel).Subscribe(...)`.

Ara la navegació és **una sola llista de set línies** dins d'un únic `WhenActivated`:

```csharp
private void Registra(CompositeDisposable d)
    => this
        .WhenAnyValue(x => x.ViewModel)
        .Where(vm => vm is not null)
        .Subscribe(vm =>
        {
            this.RegistraNavegacio<ActuacioSetWindow>(_windows, vm!.ShowActuacioSetDialog).DisposeWith(d);
            …
            this.RegistraNavegacio<UtilitatsWindow>(_windows, vm.ShowUtilitatsDialog).DisposeWith(d);
        })
        .DisposeWith(d);
```

i l'AXAML passa de `Click="Centre_OnClick"` a `Command="{Binding CentreSetCommand}"`.

**El menú i el taulell comparteixen comanda**: «Alumnes» del menú i el botó *Gestiona alumnes*
de la targeta són el mateix `AlumneSetCommand`. Abans eren dos camins independents —el menú
obria la finestra i no refrescava les xifres; la targeta sí. Ara les dues refresquen.

### R4.2 — Fitxers tocats

| Fitxer | Canvi |
|---|---|
| `UI.ER.ViewModels/ViewModels/AppStatusViewModel.cs` | Set parelles `{X}SetCommand` / `Show{X}SetDialog` (n'hi havia tres) + `UtilitatsCommand` / `ShowUtilitatsDialog`. Els tres `ShowXSetDialogHandle` idèntics es fonen en `ComandaDeNavegacio<TSortida>(Interaction<Unit,TSortida>)`. |
| `UI.ER.AvaloniaUI/Helpers/DialegExtensions.cs` | `RegistraNavegacio<TWindow>`, dues sobrecàrregues (sortida `IIdEtiquetaDescripcio?` i sortida `Unit`). |
| `UI.ER.AvaloniaUI/Views/MainWindow.axaml.cs` | −7 handlers, −3 `RegisterShowXDialog`, +1 `Registra(CompositeDisposable)`. Fora el `try { } catch { }` buit. |
| `UI.ER.AvaloniaUI/Views/MainWindow.axaml` | 7 `Click=` → `Command=`. |
| `UI.ER.AvaloniaUI.Test/NavegacioTest.cs` | Nou, 3 tests. |

**Per què una `RegistraNavegacio` nova i no `RegistraDialeg`**: `RegistraDialeg` rep el ViewModel
per l'`Interaction` (`interaction.Input`) i el passa a `GetWith`. A la navegació no hi ha cap
argument de runtime: l'entrada és `Unit` i el ViewModel el construeix la factory amb `Get<T>()`.
Són els dos costats de la mateixa moneda i per això viuen al mateix helper.

**Per què les sis llistes retornen `IIdEtiquetaDescripcio?`**: són les mateixes finestres que fan
de lookup. Obertes des del menú es tanquen sense selecció i tornen `null`, que la comanda ignora.
Mantenir-ho uniforme estalvia una segona sobrecàrrega; `UtilitatsWindow`, que no és cap llista,
és l'única amb `Interaction<Unit, Unit>`.

**El `try { } catch { }` buit de `DrawerSelectionChanged`**: eliminat. L'única línia que en
quedava dins era `PageCarousel.SelectedIndex = listBox.SelectedIndex;`, que no llança —
`SelectedIndex` fora de rang no és excepció a Avalonia. Les tres línies que sí que podien
llançar ja estaven comentades des d'abans; també fora.

### R4.3 — Tests (`UI.ER.AvaloniaUI.Test/NavegacioTest.cs`)

| Test | Què fixa |
|---|---|
| `CadaLlistaDEntitatEsArribableDesDelTaulell` | **Per comprensió** sobre `Vistes.Finestres`: cada `{X}SetWindow` ha de tenir `{X}SetCommand` i `Show{X}SetDialog` a `AppStatusViewModel`. Una entitat nova entra sola al test i obliga a donar-li entrada al menú. |
| `TotaInteraccioDelTaulellTeLaSevaComanda` | Cap `Interaction` d'`AppStatusViewModel` sense la comanda que la dispari. Una interacció òrfena només es pot llançar des del code-behind: exactament el que R4 ha tret. |
| `MainWindowNoNavegaAmbHandlersDeClick` | `MainWindow` no declara cap mètode amb la signatura exacta d'un handler de `Click` (`(_, RoutedEventArgs)`). |

**Verificat per mutació:**

| Mutació | Resultat |
|---|---|
| Tornar a posar `Centre_OnClick` que obre `CentreSetWindow` | ❌ `MainWindowNoNavegaAmbHandlersDeClick`, amb el nom del handler al missatge |
| Treure `CentreSetCommand` d'`AppStatusViewModel` | ❌ 2 tests: `…ArribableDesDelTaulell` («Entitats sense …: Centre») i `TotaInteraccio…` («… sense la comanda corresponent: ShowCentreSetDialog») |

⚠️ **Dos detalls del tercer test**, per si algú el toca:
- Filtra els mètodes amb `<` al nom: les lambdes de sincronització del calaix de navegació
  (`_navSwitch.IsCheckedChanged += (s, e) => …`) capturen `this` i el compilador les converteix
  en mètodes de `MainWindow` amb signatura `(object, RoutedEventArgs)`. Sense el filtre, el test
  falla amb `<.ctor>b__4_0`.
- Compara el segon paràmetre amb `== typeof(RoutedEventArgs)`, no amb `IsAssignableFrom`. Els
  handlers que queden (`DrawerList_KeyUp`, `DrawerSelectionChanged`,
  `TemplatedControl_OnTemplateApplied`) reben **subtipus** de `RoutedEventArgs` i han de passar.

### R4.4 — Què NO ha entrat a R4, i per què

- **`GoodbyeButtonMenuItem_OnClick`** («Sortir») i `TemplatedControl_OnTemplateApplied` es
  queden com a handlers. No naveguen: criden `SnackbarHost.Post(...)`, que és API de vista pura.
  L'excepció està escrita a `NavegacioTest.HandlersDeVista`.
  > 🐛 **De passada**: l'entrada «Sortir» **no tanca l'aplicació**, només escriu «See ya next
  > time, user!» a la snackbar (i en anglès). És codi heretat de la plantilla de Material.Avalonia.
  > Arreglar-ho és un canvi de comportament, no un refactor: queda per a **R6**, que ja ha de
  > repassar textos i diàlegs.
- **La sincronització `NavDrawerSwitch` ↔ `LeftDrawer`** i `DrawerSelectionChanged` continuen al
  code-behind. Són estat de la pròpia finestra (quin element del `Carousel` es veu, si el calaix
  és obert), no navegació: no hi ha cap finestra pel mig i el ViewModel no en sap res.
- **El quart clon de `PerCadaViewModel`.** `MainWindow` fa el seu propi
  `WhenAnyValue(x => x.ViewModel).Where(...).Subscribe(...).DisposeWith(d)` perquè no hereta de
  cap de les tres classes base de R3, que en tenen una còpia cadascuna. Extreure'l a una extensió
  demanaria tipar-la sobre `IViewFor<TVm>` i que `WhenAnyValue` continués resolent
  l'`ICreatesObservableForProperty` d'Avalonia amb el `PropertyInfo` de la interfície en comptes
  del de `ReactiveWindow<T>` — verificable només amb `Avalonia.Headless`, que avui no hi és.
  **Deute obert**, no de R4.

---

### R4.5 — Pendent de validació manual

Cap test obre una finestra: el que R4 canvia de veritat —que el menú dispari la comanda i que
el handler resolgui la finestra— només es veu executant l'aplicació.

- [ ] Obrir les **set entrades del menú** (Centres, Etapes, Cursos, Tipus actuacions, Alumnes,
      Actuacio, Utilitats) i comprovar que cadascuna obre la seva finestra i que el menú es tanca.
- [ ] Obrir «Alumnes» des del menú, donar d'alta un alumne, tancar: **les xifres del taulell
      s'han de refrescar** (abans, des del menú, no ho feien).
- [ ] Obrir i tancar la mateixa entrada **3 vegades seguides** (invariant 1 + disposal d'scope).
- [ ] Comprovar que els **tres botons de les targetes** continuen funcionant igual.

---

## R5 — Codi mort ✅ FET

> Neteja només d'esborrats: **121 línies de C# i 5 del `.csproj` fora**, cap línia afegida.
> `dotnet build` net i els 24 tests verds, sense tocar cap test: R5 no canvia cap invariant.

### R5.1 — Què s'ha esborrat

| Element | Com s'ha verificat | Resultat |
|---|---|---|
| `ViewLocator.cs` | `grep` a tot el repo: cap referència fora de la seva pròpia declaració. No està registrat a `App.axaml`. | Fitxer esborrat (32 línies) |
| `Converters/StringDateConverter.cs` | Cap `.axaml` el declara com a recurs ni hi ha cap `xmlns` cap a `UI.ER.AvaloniaUI.Converters`. Les 21 crides vives apunten totes a la còpia de `UI.ER.ViewModels/Services/StringDateConverter.cs`, que és una classe **estàtica** i no un `IValueConverter`. | Fitxer esborrat (34 línies); la carpeta `Converters/` queda buida i desapareix |
| `public OperationResult<T> Result { get; set; }` | 11 finestres Create/Update, marcades amb `// ToDo (R5)` per R3. Zero coincidències de `.Result` a `UI.ER.AvaloniaUI`, `UI.ER.AvaloniaUI.Test` i `UI.ER.ViewModels`, i cap `Binding` a `Result` a l'AXAML. | 11 propietats + els seus 11 `using BusinessLayer.Abstract;` |
| `UI.ER.AvaloniaUI.csproj` | `<AvaloniaResource Include="Assets\**" />` hi era dues vegades; `<Folder Include="Models\" />` apuntava a una carpeta que no existeix al disc. | Queda un sol `ItemGroup` amb l'`AvaloniaResource` |
| `using` no utilitzats | 11 més, veure §R5.2 | — |

`ActuacioUpdateWindow` **conserva** el seu `using BusinessLayer.Abstract;`: allà `OperationResult<T>`
és viu, com a paràmetre de `TancaSiEsborrat`.

### R5.2 — Com s'han trobat els `using` sobrants

`dotnet format --diagnostics IDE0005` no reporta res, i `IDE0005` tampoc no surt a la compilació
normal. Cal la combinació de tres coses alhora:

```bash
printf '[*.cs]\ndotnet_diagnostic.IDE0005.severity = warning\n' > UI.ER.AvaloniaUI/.editorconfig
dotnet build UI.ER.AvaloniaUI/UI.ER.AvaloniaUI.csproj --no-incremental \
  -p:GenerateDocumentationFile=true -p:NoWarn=1591 -p:EnforceCodeStyleInBuild=true
rm UI.ER.AvaloniaUI/.editorconfig
```

`GenerateDocumentationFile` és el que fa que el compilador tingui la informació semàntica que
`IDE0005` necessita; `EnforceCodeStyleInBuild` és el que carrega els analitzadors d'estil;
l'`.editorconfig` és el que puja `IDE0005` de *silent* a *warning*. Sense els tres, silenci.

⚠️ **`IDE0005` és iteratiu**: en treure un `using` en poden aparèixer de nous a la ronda següent
(a `MainWindow`, treure `Avalonia.Data` i `System.Threading.Tasks` va destapar `System.Reactive`).
Cal repetir fins que la ronda surti neta. I **filtrar per `IDE0005` de debò**: la mateixa
compilació escup `CS1573`/`CS1712`/`CS1574` dels comentaris XML, que apunten a línies de codi
real i no a `using`.

Els 11 trobats: `System.Reflection` (`DI/Injection.cs`), `Material.Styles.Controls`
(`Helpers/ConfirmationDialog.cs`), `Avalonia.Platform` (`Helpers/WindowHelper.cs`), `System`
(`Actuacio{Create,RowUserCtrl}`, `Alumne{Create,Update}Window`), `System.Reactive.Disposables`
(`AlumneInformeViewerWindow`), i `Avalonia.Data` + `System.Threading.Tasks` + `System.Reactive`
(`MainWindow`).

> La llista original d'aquest document deia que en quedaven a `DateInput` i `LookupInput`.
> **No és cert**: els dos controls estan nets. En canvi n'hi havia a `DI/Injection.cs`,
> `Helpers/ConfirmationDialog.cs`, `Helpers/WindowHelper.cs` i tres fitxers de `Pages/`,
> que la llista no esmentava.

> ⚠️ Per a **R2**: `AlumneInformeViewerWindow` ha perdut el `using System.Reactive.Disposables;`
> justament perquè encara no fa `WhenActivated`. En convertir-lo a
> `ReactiveWindow<AlumneInformeViewerViewModel>` caldrà tornar-lo a posar.

R3 ja s'havia endut per davant les altres dues entrades de la llista original: les 10 còpies de
`GetWindow()` i les 3 d'`ObraFileExplorer()`, ara cobertes per un test (§R3.5).

---

## R6 — Deriva de disseny i colors literals ✅ FET

> R6 es va fer abans que R1 perquè aleshores R1 estava bloquejat per **B3** i R6 no depenia de
> res (§3). Va ser el primer refactor que tocava de debò l'AXAML: R0–R5 gairebé no l'havien
> mirat. R1, després, hi ha tornat per resoldre B3 (§R1.5).

### R6.1 — Els tres problemes que anotava el pla, i què s'ha fet

| Problema | Solució |
|---|---|
| `CentreSetWindow.axaml` amb el disseny antic (`DockPanel` + botó `DesarCss` de 20 línies declarat inline) | Reescrita amb el mateix esquelet que les altres cinc llistes. De 89 a 73 línies |
| **96 colors literals** (`#F5F7FA`, `#E3F2FD`, `#1565C0`…) repartits per 13 AXAML i un `.cs` | `Themes/Paleta.axaml`: 23 pinzells semàntics × 2 temes. **Ara en queden 0** fora de la paleta |
| `Helpers/ConfirmationDialog.cs` muntava la UI en C#, amb `Color.Parse("#D32F2F")`, i deia sempre «Sí, esborrar» | Esborrat. El substitueix `Pages/ConfirmacioWindow.axaml` + `ConfirmacioViewModel`, amb el text afirmatiu com a paràmetre |

### R6.2 — La paleta

`UI.ER.AvaloniaUI/Themes/Paleta.axaml` és un `ResourceDictionary` amb
`ResourceDictionary.ThemeDictionaries` (`Light` i `Dark`), fusionat a `Application.Resources`.

Les claus són **semàntiques, no descriptives** — `WarningBrush`, no `Taronja` — amb tres sufixos:

| Sufix | Què és |
|---|---|
| `XxxBrush` | primer pla: text i icones de la família |
| `XxxContainerBrush` | fons del bloc que el conté |
| `XxxBorderBrush` | vora d'aquest bloc |

Set famílies: neutres (`SurfaceSubtle`, `SurfaceMuted`, `SurfaceCard`, `BorderSubtle`,
`TextSecondary`, `TextTertiary`), `Info`, `Success`, `Warning`, `Danger` (+`DangerStrong`
i `DangerStrongForeground`), `Accent` i `Header`.

**Col·lapses deliberats** en fer el mapatge: `#FFF8E1`/`#FFF3E0` → un sol `WarningContainerBrush`;
`#1976D2`/`#1565C0` → `InfoBrush`; `#FAFAFA`/`#F5F5F5` → `SurfaceMutedBrush`;
`#E53935` → `DangerStrongBrush`. Eren la mateixa idea escrita dos cops.

Els colors **amb nom** (`Foreground="Gray"`, `"Red"`, `"Blue"`, `BorderBrush="White"`) hi han
entrat igual: no els comptava el pla, però són igual de refractaris al tema fosc.

**Com passar a fosc**: canviar `RequestedThemeVariant` i `BaseTheme` a `Dark` a `App.axaml`.
Els dos junts — invariant 10. Els valors foscos ja hi són; caldrà repassar-los amb la
pantalla al davant, no s'han pogut validar.

### R6.3 — Classes d'estil noves a `App.axaml`

El mateix moviment que R3 va fer amb el C#, aplicat a l'AXAML: el que estava copiat va a
`App.axaml` i la vista només diu quina classe vol.

| Classe | Substitueix | Cops |
|---|---|---|
| `Border.Fitxa` | la targeta d'una fila (`BorderBrush` + `CornerRadius` + `Padding` + `Margin` + `BoxShadow`) | 6 |
| `Border.Apareix` / `.Desapareix` | les animacions, declarades dins d'un `<UserControl.Styles>` a cada fila | 6 |
| `Border.BarraFiltres` | la barra superior d'un llistat | 6 |
| `Border.Xip` | el xip del missatge de paginació | 6 |
| `Border.Seccio` + `.Ok` / `.Avis` / `.Perill` | les seccions dels formularis rics | 13 |
| `material:FloatingButton.Afegir` | el FAB d'alta: 14 línies d'icona + text a cada llista | 6 |
| `material:FloatingButton.Desar` (+`.Ancorat`) | el FAB de desar, que tenia **dues** versions (icona `FolderDownload` sense drecera a les 8 finestres simples, `ContentSave` + `Ctrl+S` a les 4 riques) | 12 |

Dues decisions que valen la pena:

- **El text del FAB va per `ContentTemplate`, no per `Content`.** L'estil defineix la
  plantilla (icona + `TextBlock` amb `{Binding}`) i cada finestra només escriu
  `Content="Nova etapa"`. La primera versió feia `Content` + `{Binding $parent[…].Tag}`:
  funciona igual, però depèn de com quedi l'arbre lògic dins de la plantilla i això no es
  pot comprovar amb un test. `ContentTemplate` fa que el `DataContext` de la plantilla
  sigui el `Content` mateix — cap indirecció.
- **`Border.Apareix` puja a `App.axaml` i, de retruc, arregla `MainWindow`**: la pàgina de
  llicència ja portava `Classes="Apareix"` però la finestra no definia l'estil enlloc, així
  que la classe no feia res.

**Unificació de dreceres i textos**: les 12 finestres d'edició tenen ara el mateix FAB, amb
`Ctrl+S` i el mateix tooltip. Abans les 8 simples no tenien drecera i deien «Desar els
canvis», i les 4 riques deien «Desar» amb una altra icona.

### R6.4 — Tests (`UI.ER.AvaloniaUI.Test/DissenyTest.cs`)

Cinc tests, que **escanegen el codi font** (via `[CallerFilePath]`, per no endevinar quantes
carpetes hi ha des de `bin/`). És deliberat: un color literal compila igual de bé que una
clau de recurs, i un `DynamicResource` que no resol **no peta** — el control es queda sense
color i ningú se n'assabenta fins que algú mira la pantalla.

| Test | Què impedeix |
|---|---|
| `CapAxamlEscriuUnColorLiteral` | tornar a escriure `#RRGGBB` o `Foreground="Red"` a una vista |
| `CapCodiRereLaVistaConstrueixUnColor` | tornar a muntar UI en C# amb `Color.Parse` / `new SolidColorBrush` |
| `ElsDosTemesDefineixenLesMateixesClaus` | afegir un pinzell al tema clar i oblidar-lo al fosc |
| `CadaClauQueSUsaExisteixAlaPaleta` | una errata a un `{DynamicResource …}` |
| `LaPaletaNoTeClausMortes` | que la paleta creixi amb colors que no fa servir ningú |

`ConvencioVistaViewModelTest.HiHaLesFinestresQueEsperem` passa de **21 a 22** finestres
(`ConfirmacioWindow`).

### R6.5 — Plantilles mortes esborrades

Trobades comparant cada `{Binding X}` de l'AXAML amb els membres del ViewModel que la
convenció li assigna:

- **4 `<TextBlock Text="{Binding IdTxt}"/>`** a `{Centre,CursAcademic,Etapa,TipusActuacio}CreateWindow`.
  `IdTxt` no existeix a cap `*CreateViewModel`: el `TextBlock` sortia sempre buit.
- **8 blocs `<StackPanel.Styles></StackPanel.Styles>` buits**, hereus d'un copia-i-enganxa.
- **L'estil `Button.DesarCss`** declarat dins de `CentreSetWindow` (20 línies), l'única
  finestra que el tenia.
- **Un `<Grid ColumnDefinitions="*,Auto">`** a la zona de perill d'`ActuacioUpdateWindow`
  que només embolcallava un `StackPanel`.
- **60 declaracions `xmlns:` menys.** Cap vista en declarava menys de dues que no feia
  servir. A més, l'àlies `wpf` (que apunta a `Material.Styles.Assists`, que no és WPF) i
  `assists` apuntaven al **mateix** namespace a 14 fitxers, amb `assists` sense fer servir
  mai. Ara n'hi ha un de sol, `assists`.

> **No s'ha tocat el `ToggleSwitch` de `DangerModeEnabled`** d'`ActuacioUpdateWindow`, que
> semblava desconnectat: és el `canExecute` de `DeleteCommand`, i el botó ja s'inhabilita sol.

### R6.6 — `IndicadorCarrega`

`Controls/IndicadorCarrega.axaml`: el «Carregant dades…» amb la icona giratòria, que estava
copiat idèntic a les sis llistes. Sis blocs de vuit línies passen a una línia cadascun. Té
`StyledProperty<string> Text` per si algun llistat vol un altre missatge.

### R6.7 — `ConfirmacioWindow`

- `UI.ER.ViewModels/ViewModels/ConfirmacioViewModel.cs`: `Titol`, `Missatge`,
  `TextAfirmatiu` i dues `ReactiveCommand<Unit, bool>`. **Tots els paràmetres amb valor per
  defecte**, perquè el contenidor el registri (regla de §R0.1) i, aleshores, perquè el
  `Design.DataContext` el pogués instanciar. És l'únic ViewModel que R1 no ha tocat: no
  parla amb el BusinessLayer i per tant no li cal l'`IServiceFactory` (§R1.7).
- `Pages/ConfirmacioWindow.axaml(.cs)`: `ReactiveWindow<ConfirmacioViewModel>`; el
  code-behind només tanca amb el resultat de la comanda, com fa `TancaSiDesat` a les
  classes base de R3.
- `DialegExtensions.RegistraConfirmacio(...)`: una línia al punt de crida, i la finestra
  s'obté per `IWindowFactory.GetWith` — invariant 7, cap `new XWindow(` a una vista.
- **Canvi de comportament volgut**: el botó afirmatiu ja no diu sempre «Sí, esborrar».
  Ara és el paràmetre `textAfirmatiu`, i qui obre el diàleg és qui sap de quina acció es
  tracta. L'únic punt de crida (esborrar una actuació) hi passa el mateix text d'abans.

### R6.8 — Pendent de validació manual

`dotnet build` net i `dotnet test` verd (32 tests aleshores, 40 des de R1). L'aplicació arrenca i pinta
`MainWindow` sense cap error de binding ni res a `error.log`. Els tests **no obren cap
finestra**, així que cal repassar amb la pantalla al davant:

1. Les sis llistes: barra de filtres, xip de paginació, indicador de càrrega i FAB d'alta
   amb el text correcte a cadascuna.
2. Les dotze finestres d'edició: el FAB de desar i el `Ctrl+S`.
3. `ActuacioUpdateWindow` → zona de perill → esborrar: ha de sortir el nou
   `ConfirmacioWindow` amb «Sí, esborrar», i `Esc` ha de cancel·lar.
4. `UtilitatsWindow` i `AlumneInformeViewerWindow`, que són les que més colors tenien.

### R6.9 — El que R6 **no** ha fet

- **Tema fosc de debò.** La infraestructura hi és i els valors foscos també, però ningú els
  ha vist. Activar-lo és un canvi de dues línies (invariant 10) i una repassada de contrast.
- **`AlumneInformeViewerWindow` continua sent un `Window` pelat** amb subscripcions al
  constructor: és el deute de R2, i és de subscripcions, no de disseny.
- **La duplicació que queda a les sis llistes** (l'esquelet `DockPanel` + `ItemsControl` de
  `BrokenRules` + `ListBox`) no s'ha extret a un control compost. Amb les classes d'estil ja
  són 71 línies cadascuna i el que queda és estructura, no estil; fer-ne un `UserControl`
  amb sis punts d'extensió costaria més del que estalvia.

---

## 3. Ordre recomanat

```
R0  IWindowFactory + comprensió    ✅ FET
 │
R3  classes base + helpers         ✅ FET  (-925 línies de codi rere les vistes)
 │
R2  disposal de subscripcions      ✅ FET dins de R3, tret d'AlumneInformeViewerWindow
 │
R5  neteja de codi mort            ✅ FET  (-121 línies de C#, cap afegida)
 │
R4  navegació de MainWindow        ✅ FET  (-79 línies a MainWindow, +39 al helper compartit)
 │
R6  unificació visual i paleta     ✅ FET  (-324 línies als 30 AXAML preexistents, 96 colors literals → 0)
 │
R1  eliminar SuperContext          ✅ FET  (51 crides estàtiques → 0, 27 VMs injectats, B3 resolt)
 │
R7  BusinessLayer per comprensió   ✅ FET  (30 registres a mà → 0; +3 tests)
 │
R8  compiled bindings als 19 AXAML ← següent pas; on hi ha més a guanyar és a les 12
    que R1 no ha tocat                finestres d'edició (§R1.7)
```

> R3 s'ha fet abans que R5 a petició de l'usuari. No ha costat res: R5 era «redueix soroll per
> a R3», i R3 ha reescrit igualment els 24 fitxers on hi havia el soroll.
> R2 ha entrat dins de R3 perquè escriure les classes base amb el bug a dins no tenia sentit
> (§R2).
> R5, fet després, ha estat el que preveia: només esborrats, sense tocar cap test.
> El que costava no era esborrar sinó **trobar** els `using` sobrants — veure §R5.2.
> R4 ha sortit més barat del previst perquè R0 i R3 ja hi havien deixat les dues peces:
> `IWindowFactory` i el helper de diàlegs. La feina real ha estat decidir **on posa la ratlla**
> entre navegació (va al ViewModel) i estat de la finestra (es queda al code-behind) — §R4.4.
> R6 es va avançar a R1 perquè aleshores R1 estava bloquejat per B3 i R6 no depenia de res. Ha resultat
> ser el mateix moviment de R3 però a l'AXAML: el que estava copiat 6 o 13 cops puja a
> `App.axaml` com a classe d'estil i la vista només diu quina vol. El que costava no era
> substituir els colors sinó **decidir el joc de claus**: 96 literals eren 23 idees, i unes
> quantes estaven escrites dues vegades amb valors lleugerament diferents — §R6.2.
> R1 ha sortit més barat del que el pla feia témer, i per una raó concreta: R0 i R3 ja havien
> deixat **un sol punt** on cada ViewModel es construeix. Injectar l'`IServiceFactory` als 27
> VMs és mecànic; el que calia decidir era **fins on portar-la**. La resposta ha estat «el VM
> pare la passa al fill», que estalvia tocar els contractes de R3 a canvi de deixar les
> operacions del diàleg d'edició apuntades a l'scope del pare — §R1.4. B3, l'únic blocador
> real, no ha estat cap dels 14 blocs `<Design.DataContext>` sinó el `;assembly=` que els
> `xmlns` no portaven, i que només es veu compilant amb `--no-incremental` — §R1.5.
> R7 ha estat el més petit de tots i **no estalvia línies** (43 → 49 de codi): el que compra és
> que la llista no pugui derivar mai més. L'única sorpresa ha estat que l'escaneig que proposava
> el pla no funciona tal com estava escrit — hi ha herència entre implementacions i tipus
> generats pel compilador que hi entren pel mig — §R7.2.

---

## 4. Convencions per a l'agent

- **Idioma**: comentaris de codi, missatges d'error i textos d'UI en **català**. Identificadors en anglès o català segons el que ja hi hagi al fitxer.
- **Un refactor, un commit.** No barrejar R0 amb R1.
- **No introduir dependències noves** sense preguntar. L'stack actual és: Avalonia 11.3.11, ReactiveUI.Avalonia, Material.Avalonia, Material.Icons.Avalonia, Serilog.Sinks.File.
- **`dotnet build` ha de quedar net** després de cada pas. Els tests no obren cap finestra: cada canvi estructural es valida també obrint i tancant el diàleg afectat.
- **No tocar** `BusinessLayer`, `DataLayer`, `DataModels` ni les migracions durant R0/R2/R3/R4. R7 hi ha entrat, i només a `BusinessLayer/DI/Injection.cs`. R1 hi havia de poder entrar i no ha calgut: l'`IServiceFactory` ja hi era, a `BusinessLayer.Abstract/Generic/`, i només li faltava una implementació injectable. `UI.ER.ViewModels` sí que es pot tocar: R3 hi ha afegit `Contracts/DialegContracts.cs` i R1 `Services/ServiceFactory.cs`.
- **`dotnet test UI.ER.AvaloniaUI.Test` ha de quedar verd després de cada pas.** Si un refactor canvia una invariant a consciència (p. ex. R3 introdueix classes base i el nombre de finestres es manté però els constructors canvien), s'actualitza el test amb el canvi, mai després.
- **Vistes noves**: no s'instancien amb `new`. Registrar-les no cal (l'escaneig les agafa soles), però han de complir la convenció de noms o portar `[ViewModel(typeof(...))]`, altrament l'aplicació no arrenca.
- Els fitxers `.axaml` i `.axaml.cs` van sempre junts: si es canvia l'`x:Class` o la classe base, revisar-ne els dos.
- **Colors**: cap literal. Clau de `Themes/Paleta.axaml` o del `MaterialTheme` (§R6.2). Si en cal un de nou, s'afegeix a les **dues** taules de tema.
- **Estils repetits**: si un bloc de disseny surt a més de dues vistes, va a `App.axaml` com a classe (§R6.3).
- **Atenció als finals de línia**: els finals de línia són **barrejats fitxer a fitxer** (`App.axaml`, `Views/MainWindow.axaml`, `App.axaml.cs` i uns quants ViewModels són CRLF; la resta LF), i alguns `.cs` porten BOM. Un script de reescriptura en Python els normalitza sense voler i converteix un canvi de dues línies en un diff de 478: cal detectar el final de línia del fitxer i tornar-lo a escriure en binari.
- **Mai `git checkout <fitxer>` per desfer una prova** sobre un fitxer amb feina no comitejada: se l'endú tota, no només la prova. Copiar el fitxer al directori temporal i restaurar-lo des d'allà.
- **L'AXAML es compila per separat i de forma incremental.** Un `dotnet build` net **no** vol dir que l'AXAML compili: cal `--no-incremental` per veure els `AVLN####` (§R1.5).

---

## R7 — Registre del BusinessLayer per comprensió ✅ FET

### R7.1 — Resultat

`BusinessLayer/DI/Injection.cs` llistava **30 serveis a mà** rere el comentari
`// Services (ToDo: per comprensió)`. Ara no en llista cap: els descobreix per convenció.

| | Abans | Després |
|---|---|---|
| `AddTransient<IXxx, Xxx>()` escrits a mà | 30 | **0** |
| Línies de codi de `Injection.cs` (sense comentaris ni blancs) | 43 | 49 |
| Tests a `BusinessLayer.Integration.Test` | 3 | **6** |

R7 **no estalvia línies** — n'afegeix 6 de codi i unes quantes de documentació. El que compra
és la invariant: afegir una operació al BusinessLayer ja no demana tocar el contenidor, i una
interfície nova sense implementació **peta a l'arrencada** en comptes de fer-ho en runtime dins
d'un diàleg. Els 30 registres resultants són **exactament els mateixos** que hi havia a mà
(verificat comparant les dues llistes ordenades).

> El pla parlava de «31 serveis». Són **30**: 30 fitxers a `BusinessLayer.Abstract/Services/`,
> 30 a `BusinessLayer/Services/` i 30 `AddTransient` al fitxer que hi havia abans.

### R7.2 — L'escaneig del pla no funciona, i per què

El fragment que proposava el pla —recórrer els `IBLOperation` i buscar-ne la implementació amb
`SingleOrDefault(t => contract.IsAssignableFrom(t))`— peta de tres maneres:

1. **Els contractes genèrics hi entren.** `ISet<,>`, `ICreate<>`, `IUpdate<>`, `IDelete<>` i
   `IActivaDesactiva<>` (a `BusinessLayer.Abstract.Generic`) també deriven d'`IBLOperation`, i
   com que són *open generics* cap classe no compleix `ISet<,>.IsAssignableFrom(…)`: el bucle
   llançaria «sense implementació» per cinc contractes que no s'han de registrar.
   → El filtre és el **namespace** (`BusinessLayer.Abstract.Services`), no només l'herència.
2. **Hi ha herència entre implementacions.** `CentreSetAmbActuacions : CentreSet` fa que
   `ICentreSet.IsAssignableFrom(…)` sigui cert per **dues** classes, i el `SingleOrDefault`
   llança *«Sequence contains more than one element»*.
   → La parella es busca **pel nom** (`IXxx` → `Xxx`); l'assignabilitat es queda com a
   validació, no com a criteri de cerca.
3. **Els tipus generats pel compilador comparteixen namespace.** Una primera versió indexava
   les classes de `BusinessLayer.Services` per nom amb `ToDictionary(t => t.Name)` i petava amb
   *«An item with the same key has already been added. Key: `<>c`»*: les classes de closures que
   genera el compilador per cada lambda són tipus imbricats i `Namespace` els retorna el del
   tipus que les declara.
   → No es fa cap diccionari: es demana el tipus concret amb
   `Assembly.GetType($"{ns}.{nom}")`.

### R7.3 — Com queda

```csharp
public static IServiceCollection BusinessLayerConfigureServices(this IServiceCollection services)
{
    foreach (var (contracte, implementacio) in Operacions())
        services.AddTransient(contracte, implementacio);

    return services;
}

internal static IEnumerable<Type> Contractes()
    => typeof(IBLOperation).Assembly.GetTypes()
        .Where(t => t.IsInterface
                 && t.Namespace == typeof(ICentreSet).Namespace
                 && typeof(IBLOperation).IsAssignableFrom(t))
        .OrderBy(t => t.Name);
```

`Operacions()` aparella cada contracte amb `BusinessLayer.Services.{nom sense la I}`, acumula
**tots** els errors i llança un sol `InvalidOperationException` que els llista. Acumular en
comptes de petar al primer és el mateix criteri que `ValidaConvencioVistaViewModel()` a
`UI.ER.AvaloniaUI/DI/Injection.cs` (R0): qui hi afegeix serveis els veu tots de cop.

### R7.4 — El cicle de vida no canvia

`AddTransient`, com abans. L'avís del pla sobre la fuita d'`IBLOperation : IDisposable` retingut
pel provider **arrel** el va tancar R1 i R7 no hi torna: l'`IServiceFactory` és `AddScoped`, els
VMs la reben pel constructor i les operacions queden apuntades a l'scope del diàleg (§R1.2).
El test `CadaContracteTeLaSevaImplementacioPerConvencio` fixa el `Transient` explícitament perquè
un canvi de cicle de vida no hi entri per descuit.

### R7.5 — Tests (`BusinessLayer.Integration.Test/InjeccioTest.cs`, 3 → 6)

| Test | Què fixa |
|---|---|
| `CadaContracteTeLaSevaImplementacioPerConvencio` | cada contracte té **un** registre, amb la implementació de nom convencional i `Transient` |
| `NomesEsRegistrenLesOperacionsDelNamespaceServices` | l'escaneig no arrossega ni els contractes genèrics ni les classes base de `BusinessLayer/Common` |
| `TotesLesOperacionsEsResolen` | les 30 es construeixen de debò des del provider (`validateScopes: true`), amb un `IDbContextFactory<AppDbContext>` de SQLite |

Els tests no toquen els `internal` de `Injection`: llegeixen l'`IServiceCollection` i el provider,
que és el que veuen els consumidors. El tercer és el que hauria detectat l'`ImportAll`, l'única
operació que en depèn de sis més pel constructor.

`dotnet test BusinessLayer.Integration.Test` → **6/6**.
`dotnet test UI.ER.AvaloniaUI.Test` → **40/40** (sense canvis).
`dotnet build eaprecull.sln` → net.

### R7.6 — El que R7 **no** ha fet

- **No toca els altres tres punts d'entrada** (`UI.ER.AvaloniaUI/App.axaml.cs`,
  `ImportData/Program.cs`, `CreateDemoData/Program.cs`): tots tres criden
  `BusinessLayerConfigureServices()` i no s'han d'assabentar del canvi.
- **No toca `DataLayer/DI/Injection.cs`**, que registra una sola cosa i no té llista a treure.
  (Hi queda, això sí, el `BuildServiceProvider()` que fa dins del mètode de registre per aplicar
  les migracions — un provider intermedi que es llença; és una altra conversa.)
- **No canvia el cicle de vida de res** (§R7.4).

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
| R1 — eliminar `SuperContext` | ⬜ **següent** (51 crides vives; cal **B3** abans) |
| R7 — registre del BusinessLayer per comprensió | ⬜ pendent |

---

## 1. Estat actual de `UI.ER.AvaloniaUI`

**Volum després de R3, R5, R4 i R6**: el codi rere les vistes (`*.axaml.cs`) ha passat de **2.233 a 1.239 línies**
(−1.056 de R3/R5/R4, +62 de R6: dues vistes noves), a canvi de **407 línies** de codi compartit: tres classes base
(`Pages/Base/`), tres helpers (`Helpers/{DialegExtensions,FileExplorer,VisualRootExtensions}.cs`)
i les tres interfícies de contracte dels ViewModels
(`UI.ER.ViewModels/ViewModels/Contracts/DialegContracts.cs`).
Tot el projecte suma **2.022 línies** de `.cs`.

L'AXAML, intocat fins a R4, és el que ha mogut R6: els **30 fitxers preexistents** han passat de
**3.189 a 2.865 línies** (−324) tot i que `App.axaml` n'hi ha guanyat 139, i n'hi ha **3 de nous**
(`Themes/Paleta.axaml`, `Controls/IndicadorCarrega.axaml`, `Pages/ConfirmacioWindow.axaml`, 165 línies).
Total: **33 fitxers, 3.030 línies**. Els **96 colors literals** són **0**.
`UI.ER.AvaloniaUI.Test`: 9 fitxers, 32 tests.

### Patrons en ús

| Patró | On es materialitza |
|---|---|
| **MVVM + ReactiveUI** | `ReactiveWindow<TVm>` / `ReactiveUserControl<TVm>` + `WhenActivated` |
| **Interaction pattern** | `vm.ShowXDialog.RegisterHandler(...)` — el VM demana un diàleg sense conèixer Avalonia |
| **Navegació per comanda** | `Command="{Binding XSetCommand}"` a l'AXAML + `RegistraNavegacio<XWindow>` a la vista (R4) |
| **Composition Root** | `App.OnFrameworkInitializationCompleted` → `DataLayerConfigureServices()` + `BusinessLayerConfigureServices()` + `UIConfigureServices()` |
| **Factory de vistes** | `IWindowFactory.Get<T>()` / `GetWith<T>(dc)` — cap `new XWindow(` al codi (R0) |
| **Classes base genèriques** | `EntityEditWindow<TVm,TDto>`, `EntitySetWindow<…>`, `EntityRowUserCtrl<…>` a `Pages/Base/` (R3) |
| **Contractes de ViewModel** | `ISubmitViewModel<TDto>`, `ISetViewModel<…>`, `IRowViewModel<…>` — el que les classes base poden donar per fet (R3) |
| **Registre per comprensió** | `UI.ER.AvaloniaUI/DI/Injection.cs` escaneja finestres i ViewModels |
| **Service Locator estàtic** | `SuperContext.Resolve<T>()` — **51 crides** des dels ViewModels (viu fins a R1) |
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

| Grup | Signatura | Resoluble per DI sense args? |
|---|---|---|
| `{Alumne,Centre,CursAcademic,Etapa,TipusActuacio}CreateViewModel` | `()` | ✅ Sí |
| `AppStatusViewModel`, `UtilitatsViewModel` | `()` | ✅ Sí |
| `ConfirmacioViewModel` | `(string titol = …, string missatge = …, string textAfirmatiu = …)` | ✅ Sí (tot amb default) |
| `ActuacioCreateViewModel` | `(int? alumneId = null)` | ✅ Sí (té default) |
| `{Alumne,Centre,CursAcademic,Etapa,TipusActuacio}SetViewModel` | `(bool modeLookup = false)` | ⚠️ Sí, però cal `true` en 16 punts |
| `ActuacioSetViewModel` | `(bool modeLookup = false, int? alumneId = null)` | ⚠️ Igual |
| `{…}UpdateViewModel` (6) | `(int id)` | ❌ Requereix arg de runtime |
| `AlumneInformeViewerViewModel` | `(int alumneId)` | ❌ Requereix arg de runtime |
| `{…}RowViewModel` (6) | `(DTO data, …, bool modeLookup)` | ❌ Fora d'abast (per ítem) |

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
4. `Design.DataContext` (14 blocs a l'AXAML) instancia el VM amb **constructor sense paràmetres**. Afegir injecció per constructor als VMs trencarà aquests blocs. *(B3 — encara pendent, blocador de R1.)*
5. **`UI.ER.AvaloniaUI.Test` cobreix les invariants estructurals de R0, R3 i R4** (convenció, registre, cicles de vida, constructors, herència de les classes base, navegació per comanda). No cobreix res que necessiti una plataforma d'Avalonia ni la base de dades: això es valida amb `dotnet build` + prova manual. Veure §R0.9 i §R3.5.
6. **Tota vista ha de conservar un constructor `public` sense paràmetres.** Els `*RowUserCtrl` perquè els instancia el `ListBox.ItemTemplate`; les finestres perquè, si no, el compilador d'Avalonia emet `AVLN3001`. Les que necessiten serveis fan servir el constructor pont encadenat sobre `App.Services` (§R0.5).
7. **Les vistes ja no fan `new` d'altres vistes.** Tot passa per `IWindowFactory`. La validació d'arrencada de `DI/Injection.cs` peta si s'afegeix una finestra fora de convenció sense `[ViewModel(typeof(...))]`.
8. **Cada vista amb `x:Class` conserva el seu `InitializeComponent()`.** `AvaloniaXamlLoader.Load(this)` es queda a la classe derivada, mai a la classe base: el compilador d'Avalonia el reescriu a una crida directa al mètode generat només quan el troba dins del tipus que declara l'AXAML. Pujar-lo a la base compilaria igual però passaria a resoldre's per reflexió en temps d'execució.
9. **Els escanejos de `DI/Injection.cs` i de `Vistes.cs` filtren `IsAbstract` i `IsGenericTypeDefinition`.** És el que manté les classes base de R3 fora del registre i fora dels tests d'inventari. No treure aquests filtres.
10. **`RequestedThemeVariant` (App.axaml) i `BaseTheme` (`MaterialTheme`) han d'anar sempre iguals.** El primer tria quina taula de `Themes/Paleta.axaml` s'aplica; el segon, la del `MaterialTheme`. Si es deixa `RequestedThemeVariant` sense fixar, Avalonia segueix el tema del sistema operatiu i la paleta pròpia se'n va a fosc mentre Material es queda clar. Canviar de tema és tocar-los tots dos.
11. **Cap color s'escriu a pèl.** Tot surt d'una clau de `Themes/Paleta.axaml` o del `MaterialTheme`. Ho vigila `DissenyTest` (§R6.4), que escaneja el *codi font* — un literal compila igual de bé que una clau, i el que es vol vigilar és què s'escriu.
12. **Cap vista navega des d'un handler de `Click`.** Cada entrada de menú i cada botó que obre una finestra és una `ICommand` del ViewModel amb la seva `Interaction`; la vista només diu quina finestra l'atén (`RegistraNavegacio<TWindow>`). Ho vigila `NavegacioTest` (§R4.3). Els handlers que queden a `MainWindow` no naveguen: escriuen a la snackbar o mouen el `Carousel`.

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

Composition root actual:

```csharp
var services = new ServiceCollection()
    .DataLayerConfigureServices()
    .BusinessLayerConfigureServices()
    .UIConfigureServices();          // ← nou

services.AddSingleton<IServiceFactory, SuperContext>();   // desapareix a R1

_services = services.BuildServiceProvider();
SuperContext.Initialize(_services);                        // desapareix a R1

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

⚠️ **Honestedat sobre la fuita**: la infraestructura hi és, però **encara no arregla res**.
Els ViewModels continuen demanant els serveis BL amb `SuperContext.Resolve<T>()`, que resol
**al provider arrel** — fora de l'scope del diàleg. La llista de disposables de l'arrel segueix
creixent igual que abans.

L'scope només tindrà efecte quan **R1** faci que els ViewModels rebin els serveis pel constructor:
- Amb `Get<T>()` ja funcionarà sol: el VM es resol de `scope.ServiceProvider`.
- Amb `GetWith<T>(dc)` **no**, perquè el VM ja arriba construït des de fora.
  → R1 haurà d'afegir `Get<TWindow>(params object[] vmArgs)` que faci
  `ActivatorUtilities.CreateInstance(scope.ServiceProvider, vmType, vmArgs)`, i migrar-hi
  els punts de crida amb `modeLookup: true` i els `interaction.Input`.

### R0.4 — API i quan fer servir cada operació

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

`Get<T>()` només funciona si el ViewModel està registrat, és a dir si tots els paràmetres del
seu constructor tenen valor per defecte. Per a la resta, `GetWith`.

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

> **Per què no `Get<T>(params object[] args)` avui**: `ActivatorUtilities.CreateInstance` fa el
> matching per tipus i no gestiona bé els `int?` (`ActuacioSetViewModel(bool, int?)`,
> `ActuacioCreateViewModel(int?)`). A més seria posicional i opac. Veure R1.

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
| **B3** | 13 blocs `<Design.DataContext>` | ⏭️ **Ajornat a R1.** R0 no els toca perquè no calia: cap `Window` s'instancia des de l'AXAML i els `RowUserCtrl` conserven constructor sense paràmetres. **Serà blocador real de R1**, quan els VMs rebin injecció per constructor. Migrar a `x:DataType` + `x:CompileBindings="True"`. Nota: `AlumneInformeViewerWindow.axaml` ja està trencat avui (`AlumneInformeViewerViewModel(int alumneId)` no té constructor buit). |
| **B4** | `AlumneInformeViewerWindow` és `Window` pelat, no `ReactiveWindow<T>` | ⏭️ **Sense canvis.** Funciona amb la factory (fa servir `DataContext`). Unificar-lo a R2. |
| **B5** | Lifetime | ✅ **`AddTransient` per a totes les vistes**, verificat. |
| **B6** | `SuperContext.Resolve<T>()` | ⏭️ **Viu, 51 crides.** R0 només tanca el `new` de les *vistes*. R1. |

### R0.7 — Criteris d'acceptació

- [x] Cap `new XWindow(` als fitxers `.cs` de `UI.ER.AvaloniaUI` (43 punts de crida eliminats). L'excepció que quedava —el `new Window` anònim de `Helpers/ConfirmationDialog.cs`— ha desaparegut a **R6**: ara és `Pages/ConfirmacioWindow.axaml`, resolta per la factory.
- [x] Cap registre de vista escrit a mà a `App.axaml.cs` — tot per escaneig a `DI/Injection.cs`.
- [x] Validació d'arrencada que itera totes les finestres i comprova que `ViewModelTypeFor` resol.
- [x] **Tests de regressió**: `UI.ER.AvaloniaUI.Test`, 19 tests verds, verificats per mutació (§R0.9).
- [x] Cada diàleg allibera el seu `IServiceScope` a `Closed` *(cablejat; sense efecte pràctic fins a R1, veure R0.3)*.
- [x] `dotnet build eaprecull.sln --no-incremental` net: **0 errors i cap warning d'Avalonia, de C# ni dels analitzadors**. Els 100 warnings són tots `NU1903` de vulnerabilitats de paquets (82 abans de R0; els 18 nous els aporta el projecte de tests, que arrossega els mateixos paquets).
- [x] L'aplicació arrenca sense excepcions (`error.log` buit).
- [ ] **Prova manual pendent de l'usuari**: obrir i tancar el mateix diàleg 3 vegades seguides (valida B5 i el disposal d'scope), i recórrer els lookups d'`Actuacio{Create,Update}` i `Alumne{Create,Update}`.

### R0.9 — Tests de regressió (`UI.ER.AvaloniaUI.Test`)

Projecte xUnit nou, amb els mateixos paquets que `BusinessLayer.Integration.Test`.
**19 tests, ~60 ms, sense base de dades i sense plataforma gràfica.**

> ⚠️ `DataLayerConfigureServices()` **executa les migracions** com a efecte secundari del
> registre. Els tests només criden `UIConfigureServices()`, que no toca ni disc ni BD.

| Fitxer | Què fixa |
|---|---|
| `Vistes.cs` | Punt únic des d'on s'enumeren vistes i ViewModels reals. **Cap test escriu llistes de tipus a mà**: una vista nova entra sola a tots els tests. |
| `ConvencioVistaViewModelTest.cs` | Cada finestra resol el seu ViewModel; l'atribut guanya a la convenció; el missatge d'error diu què s'esperava i com arreglar-ho; `UIConfigureServices()` no llança. |
| `RegistreDITest.cs` | Totes les finestres registrades i `Transient`; es registren **exactament** els ViewModels construïbles sense arguments; els d'arguments de runtime **no** hi són; les dependències de cada constructor de finestra existeixen; `IWindowFactory` és `Singleton`. |
| `ConstructorsDeVistaTest.cs` | Tota vista té constructor públic sense paràmetres (AVLN3001 + `ItemTemplate`); **el constructor pont encadena de debò amb el de DI** (inspecció de l'IL: busca el `call` al token de l'altre constructor); tota vista amb un camp `IWindowFactory` té el constructor que el rep. |
| `WindowFactoryTest.cs` | `GetWith` rebutja un ViewModel que no és el de la finestra, i ho fa **abans** de construir res. |

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
2. ~~Els 16 punts de lookup fent `new XSetViewModel(modeLookup: true)` a mà~~ → ✅ **R3**: `RegistraLookup<TSetWindow>`, una línia per lookup. El `new` hi continua sent, com a `Func<ViewModelBase>`; el mata **R1**.
3. `App.Services` als 16 constructors pont → ❌ **no reduïble**, veure §R0.5.
4. La signatura `Get<TWindow>(params object[] vmArgs)` que substituirà els `GetWith` → **R1**.

---

## R1 — Eliminar el Service Locator estàtic `SuperContext`

**Estat**: `IServiceFactory` està registrat a `App.axaml.cs:27` però **no s'injecta enlloc**. És una abstracció morta. Els 51 `SuperContext.Resolve<T>()` fan que cap ViewModel sigui construïble ni testejable.

**Objectiu**: injectar `IServiceFactory` (o els serveis BL concrets) pel constructor dels VMs.

**Dependència**: R0 ✅, R3 ✅ i R4 ✅ fets. La factory ja és qui construeix els VMs sense arguments (`Get<T>()`, des de l'scope del diàleg); falta la variant amb arguments de runtime.

**Punt de partida després de R4 i R6**: **51 crides** a `SuperContext.Resolve<T>()` repartides per **26 fitxers**. R6 no n'ha tocat cap — l'únic ViewModel que hi ha afegit, `ConfirmacioViewModel`, no parla amb el BusinessLayer i per tant tampoc no entra a R1. R4 no n'ha tocat cap —només ha mogut *qui dispara* els diàlegs—, però hi deixa dues coses a favor:
- Les **set finestres de navegació** de `MainWindow` s'obren totes amb `Get<T>()`, no amb `GetWith`. Són les primeres que veuran l'scope per diàleg funcionar de debò, sense cap canvi al punt de crida (§R0.3).
- `AppStatusViewModel` ha quedat com el cas de prova més net per començar: tres `SuperContext.Resolve<T>()` a `LoadData()`, cap argument de constructor, i `ComandaDeNavegacio` ja aïlla tota la navegació de la càrrega de dades.

**Disseny acordat** (discussió del 2026-09-03):

- Els VMs reben **`IServiceFactory`** pel constructor, no els serveis BL concrets. No és opcional: les operacions BL són `AddTransient` i `IBLOperation : IDisposable`, i els VMs les consumeixen amb `using var bl = …` a cada crida. Una instància injectada quedaria disposada després del primer ús. Ha de ser una fàbrica: `IServiceFactory` o `Func<IXxx>`.
- `IServiceFactory` **no és un `IServiceProvider` disfressat**: `T GetBLOperation<T>() where T : IBLOperation` només pot arribar a operacions de BL.
- **`SuperContext` desapareix del tot.** El substitueix `ServiceFactory(IServiceProvider provider)` registrat com a **`AddScoped`**. Avui `SuperContext` resol des del provider **arrel**, i MS.DI registra allà els transitoris `IDisposable` per disposar-los al final: les 51 crides deixen una referència viva fins que es tanca l'aplicació. Injectar una fàbrica *scoped* ho arregla i fa que l'scope per diàleg de §R0.3 tingui efecte de debò.
- Els 4 *seams* `protected virtual IXxx BLxxx()` (a `Actuacio{Create,Update}` i `AlumneCreate`) s'esborren: amb la fàbrica injectada ja no calen, i a més només cobrien l'operació d'escriptura, no les càrregues.
- El test d'un VM passa a ser un `FakeBL : IServiceFactory` amb un diccionari, sense estat global.
- Si més endavant un VM concret demana constructors més explícits, migrar-lo a `Func<IXxx>` és un canvi d'un sol fitxer, perquè la fàbrica ja hi arriba pel constructor.

**Feina concreta que R0 deixa preparada**:
1. Afegir `TWindow Get<TWindow>(params object[] vmArgs)` a `IWindowFactory`, implementat amb
   `ActivatorUtilities.CreateInstance(scope.ServiceProvider, WindowFactory.ViewModelTypeFor(typeof(TWindow)), vmArgs)`.
   Compte amb els `int?` (`ActuacioSetViewModel(bool, int?)`): cal treure'ls dels constructors abans, o passar-hi un tipus explícit.
2. Migrar els 16 lookups a la nova sobrecàrrega. Des de R3 són 16 crides a
   `RegistraLookup<XSetWindow>(…, () => new XSetViewModel(modeLookup: true))` repartides per
   `Actuacio{Create,Update}Window` i `Alumne{Create,Update}Window`: el `Func<ViewModelBase>`
   desapareix i `RegistraLookup` passa a demanar només el tipus de finestra i els arguments.
3. Els `GetWith<XUpdateWindow>(interaction.Input)` són un cas diferent: el VM el construeix el `*SetViewModel`. O bé el VM pare rep una `Func<int, XUpdateViewModel>` injectada, o bé la `Interaction` passa a portar l'`id` en comptes del VM sencer (més net: el VM pare deixa de construir VMs).
4. Només llavors l'scope per diàleg de R0.3 comença a alliberar serveis de debò.

**Trampes**:
- Trencarà els 14 `<Design.DataContext>` → resoldre B3 primer (migrar a `x:DataType` + `x:CompileBindings="True"`). *(R6 n'hi ha afegit un, el de `ConfirmacioWindow`; en canvi els 4 `IdTxt` morts que hauria calgut arreglar en compilar els bindings ja no hi són — §R6.5.)*
- El filtre de registre de ViewModels a `DI/Injection.cs` (§R0.1) mira *«tots els paràmetres tenen valor per defecte»*. Quan els VMs rebin serveis pel constructor deixarà de valer: caldrà canviar-lo per *«tots els paràmetres són resolubles pel contenidor»*.
- Els `{…}RowViewModel` es creen en bucle dins dels `*SetViewModel`; caldrà un `Func<TDto, bool, TRowVm>` injectat, o passar la `IServiceFactory` avall.
- Les tres interfícies de `Contracts/DialegContracts.cs` (R3) **no** es toquen: no diuen res de com el VM obté els seus serveis. Les classes base de R3 continuen valent tal com són.
- `RegistraNavegacio` (R4) **tampoc**: ja passa per `Get<T>()`, que resol des de l'scope. Qui canvia és `RegistraLookup`, que és l'únic helper amb un `new` a dins.
- Abast: 2 projectes, ~30 fitxers. **Fer-ho en un commit separat.**

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

> R6 s'ha fet abans que R1 perquè R1 continua bloquejat per **B3** i R6 no depèn de res
> (§3). És també l'únic refactor que toca de debò l'AXAML: R0–R5 gairebé no l'havien mirat.

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
  defecte**, perquè el contenidor el registri (regla de §R0.1) i el `Design.DataContext`
  el pugui instanciar.
- `Pages/ConfirmacioWindow.axaml(.cs)`: `ReactiveWindow<ConfirmacioViewModel>`; el
  code-behind només tanca amb el resultat de la comanda, com fa `TancaSiDesat` a les
  classes base de R3.
- `DialegExtensions.RegistraConfirmacio(...)`: una línia al punt de crida, i la finestra
  s'obté per `IWindowFactory.GetWith` — invariant 7, cap `new XWindow(` a una vista.
- **Canvi de comportament volgut**: el botó afirmatiu ja no diu sempre «Sí, esborrar».
  Ara és el paràmetre `textAfirmatiu`, i qui obre el diàleg és qui sap de quina acció es
  tracta. L'únic punt de crida (esborrar una actuació) hi passa el mateix text d'abans.

### R6.8 — Pendent de validació manual

`dotnet build` net i `dotnet test` verd (32 tests). L'aplicació arrenca i pinta
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
R1  eliminar SuperContext          ← següent pas: commit separat, travessa 2 projectes; cal B3 abans
 │
R7  BusinessLayer per comprensió   ← natural just després de R1
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
> R6 s'ha avançat a R1 perquè R1 continua bloquejat per B3 i R6 no depenia de res. Ha resultat
> ser el mateix moviment de R3 però a l'AXAML: el que estava copiat 6 o 13 cops puja a
> `App.axaml` com a classe d'estil i la vista només diu quina vol. El que costava no era
> substituir els colors sinó **decidir el joc de claus**: 96 literals eren 23 idees, i unes
> quantes estaven escrites dues vegades amb valors lleugerament diferents — §R6.2.

---

## 4. Convencions per a l'agent

- **Idioma**: comentaris de codi, missatges d'error i textos d'UI en **català**. Identificadors en anglès o català segons el que ja hi hagi al fitxer.
- **Un refactor, un commit.** No barrejar R0 amb R1.
- **No introduir dependències noves** sense preguntar. L'stack actual és: Avalonia 11.3.11, ReactiveUI.Avalonia, Material.Avalonia, Material.Icons.Avalonia, Serilog.Sinks.File.
- **`dotnet build` ha de quedar net** després de cada pas. Els tests no obren cap finestra: cada canvi estructural es valida també obrint i tancant el diàleg afectat.
- **No tocar** `BusinessLayer`, `DataLayer`, `DataModels` ni les migracions durant R0/R2/R3/R4. R1 i R7 sí que hi entren. `UI.ER.ViewModels` sí que es pot tocar: R3 hi ha afegit `Contracts/DialegContracts.cs`.
- **`dotnet test UI.ER.AvaloniaUI.Test` ha de quedar verd després de cada pas.** Si un refactor canvia una invariant a consciència (p. ex. R3 introdueix classes base i el nombre de finestres es manté però els constructors canvien), s'actualitza el test amb el canvi, mai després.
- **Vistes noves**: no s'instancien amb `new`. Registrar-les no cal (l'escaneig les agafa soles), però han de complir la convenció de noms o portar `[ViewModel(typeof(...))]`, altrament l'aplicació no arrenca.
- Els fitxers `.axaml` i `.axaml.cs` van sempre junts: si es canvia l'`x:Class` o la classe base, revisar-ne els dos.
- **Colors**: cap literal. Clau de `Themes/Paleta.axaml` o del `MaterialTheme` (§R6.2). Si en cal un de nou, s'afegeix a les **dues** taules de tema.
- **Estils repetits**: si un bloc de disseny surt a més de dues vistes, va a `App.axaml` com a classe (§R6.3).
- **Atenció als finals de línia**: `App.axaml` i `Views/MainWindow.axaml` són CRLF, la resta LF. Un script de reescriptura en Python els normalitza sense voler i converteix un canvi de dues línies en un diff de 478.

---

## R7 — (Relacionat) Registre del BusinessLayer per comprensió

`BusinessLayer/DI/Injection.cs` ja porta el comentari `// Services (ToDo: per comprensió)` i llista **31 serveis a mà**.

La convenció és perfectament regular: cada `IXxx` a `BusinessLayer.Abstract.Services` té la implementació `Xxx` a `BusinessLayer.Services`.

```csharp
var abstractAsm = typeof(IBLOperation).Assembly;
var implAsm     = typeof(CentreSet).Assembly;

foreach (var contract in abstractAsm.GetTypes()
             .Where(t => t.IsInterface && typeof(IBLOperation).IsAssignableFrom(t) && t != typeof(IBLOperation)))
{
    var impl = implAsm.GetTypes()
        .SingleOrDefault(t => !t.IsAbstract && contract.IsAssignableFrom(t));

    if (impl is null)
        throw new InvalidOperationException($"Sense implementació per a {contract.Name}");

    services.AddTransient(contract, impl);
}
```

Llançar en comptes d'ignorar silenciosament: així afegir una interfície sense implementació peta a l'arrencada, no en runtime dins d'un diàleg.

⚠️ **Recordatori**: `IBLOperation : IDisposable`. Combinat amb `AddTransient` i resolució des del provider **arrel**, el contenidor reté totes les instàncies creades fins que es tanca l'aplicació — encara que el `using var bl = …` cridi `Dispose()`. És una fuita real i creixent. La resol l'`IServiceScope` per diàleg de R0.3.

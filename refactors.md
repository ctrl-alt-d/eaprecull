# Refactors pendents — UI.ER.AvaloniaUI

> **Propòsit**: document de context per a agents IA durant el refactor de la capa de presentació.
> Complementa `agents.md` (que descriu l'arquitectura *actual* de tota la solució).
> Branca de treball: `refactorDI`.

## Estat del pla

| Refactor | Estat |
|---|---|
| **R0** — `IWindowFactory` + registre per comprensió | ✅ **FET** — veure §R0 |
| R5 — codi mort | ⬜ pendent |
| R3 — classes base + helpers | ⬜ pendent (ja pot consumir `IWindowFactory`) |
| R2 — disposal de subscripcions | ⬜ pendent |
| R4 — navegació de `MainWindow` | ⬜ pendent (ja pot consumir `IWindowFactory`) |
| R1 — eliminar `SuperContext` | ⬜ pendent (51 crides vives) |
| R6 — unificació visual | ⬜ pendent |
| R7 — registre del BusinessLayer per comprensió | ⬜ pendent |

---

## 1. Estat actual de `UI.ER.AvaloniaUI`

**Volum**: 40 fitxers `.cs` (2.789 línies) + 30 fitxers `.axaml` (3.189 línies), més `UI.ER.AvaloniaUI.Test` (6 fitxers).
*(Els 4 fitxers nous són els de R0: `Services/{IWindowFactory,WindowFactory,ViewModelAttribute}.cs` i `DI/Injection.cs`.)*

### Patrons en ús

| Patró | On es materialitza |
|---|---|
| **MVVM + ReactiveUI** | `ReactiveWindow<TVm>` / `ReactiveUserControl<TVm>` + `WhenActivated` |
| **Interaction pattern** | `vm.ShowXDialog.RegisterHandler(...)` — el VM demana un diàleg sense conèixer Avalonia |
| **Composition Root** | `App.OnFrameworkInitializationCompleted` → `DataLayerConfigureServices()` + `BusinessLayerConfigureServices()` + `UIConfigureServices()` |
| **Factory de vistes** | `IWindowFactory.Get<T>()` / `GetWith<T>(dc)` — cap `new XWindow(` al codi (R0) |
| **Registre per comprensió** | `UI.ER.AvaloniaUI/DI/Injection.cs` escaneja finestres i ViewModels |
| **Service Locator estàtic** | `SuperContext.Resolve<T>()` — **51 crides** des dels ViewModels (viu fins a R1) |
| **Attached behavior** | `WindowHelper.ClampToWorkingArea` aplicada globalment amb `Style Selector="Window"` |
| **Custom controls** | `DateInput`, `LookupInput` amb `StyledProperty` |
| **Styling per classes** | `LookupCss`, `ClearCss`, `EditarCss`, `InformeCss`, `PivotCss`… a `App.axaml` |
| **Convenció CRUD** | `X{Create,Update,Set}Window` + `XRowUserCtrl`, per a 6 entitats |

### Inventari de vistes

**21 `Window`**:
`MainWindow` · `{Actuacio,Alumne,Centre,CursAcademic,Etapa,TipusActuacio}CreateWindow` ·
`{…}UpdateWindow` (6) · `{…}SetWindow` (6) · `UtilitatsWindow` · `AlumneInformeViewerWindow`

**6 `UserControl`**: `{Actuacio,Alumne,Centre,CursAcademic,Etapa,TipusActuacio}RowUserCtrl`
*(instanciats pel `ListBox.ItemTemplate`, amb `DataContext` heretat de l'`ItemsSource` — el contenidor no els construeix; obtenen la factory pel constructor pont, veure §R0.5)*

### Signatures dels ViewModels (determinen què pot fer la factory)

| Grup | Signatura | Resoluble per DI sense args? |
|---|---|---|
| `{Alumne,Centre,CursAcademic,Etapa,TipusActuacio}CreateViewModel` | `()` | ✅ Sí |
| `AppStatusViewModel`, `UtilitatsViewModel` | `()` | ✅ Sí |
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
4. `Design.DataContext` (13 blocs a l'AXAML) instancia el VM amb **constructor sense paràmetres**. Afegir injecció per constructor als VMs trencarà aquests blocs. *(B3 — encara pendent, blocador de R1.)*
5. **`UI.ER.AvaloniaUI.Test` cobreix les invariants estructurals de R0** (convenció, registre, cicles de vida, constructors). No cobreix res que necessiti una plataforma d'Avalonia ni la base de dades: això es valida amb `dotnet build` + prova manual. Veure §R0.9.
6. **Tota vista ha de conservar un constructor `public` sense paràmetres.** Els `*RowUserCtrl` perquè els instancia el `ListBox.ItemTemplate`; les finestres perquè, si no, el compilador d'Avalonia emet `AVLN3001`. Les que necessiten serveis fan servir el constructor pont encadenat sobre `App.Services` (§R0.5).
7. **Les vistes ja no fan `new` d'altres vistes.** Tot passa per `IWindowFactory`. La validació d'arrencada de `DI/Injection.cs` peta si s'afegeix una finestra fora de convenció sense `[ViewModel(typeof(...))]`.

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
R3 el pot reduir a 4 punts, un per classe base (`EntityCreateWindow<…>`, `EntityUpdateWindow<…>`,
`EntitySetWindow<…>`, `EntityRowUserCtrl<…>`).

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

- [x] Cap `new XWindow(` als fitxers `.cs` de `UI.ER.AvaloniaUI` (43 punts de crida eliminats; només queda `new Window` dins de `Helpers/ConfirmationDialog.cs`, que és un `Window` anònim construït en C# i que R6 ha de convertir en AXAML).
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

### R0.8 — Deutes que R0 deixa oberts

1. `GetWindow() => (Window)this.VisualRoot!` continua duplicat a 10 fitxers → `TopLevelExtensions.GetOwnerWindow()` a **R3**.
2. Els 16 punts de lookup segueixen fent `new XSetViewModel(modeLookup: true)` a mà → helper `RegisterLookup<TWindow>` a **R3**.
3. `App.Services` com a localitzador estàtic als 17 constructors pont → reduir a 4 punts amb les classes base de **R3**.
4. La signatura `Get<TWindow>(params object[] vmArgs)` que substituirà els `GetWith` → **R1**.

---

## R1 — Eliminar el Service Locator estàtic `SuperContext`

**Estat**: `IServiceFactory` està registrat a `App.axaml.cs:27` però **no s'injecta enlloc**. És una abstracció morta. Els 51 `SuperContext.Resolve<T>()` fan que cap ViewModel sigui construïble ni testejable.

**Objectiu**: injectar `IServiceFactory` (o els serveis BL concrets) pel constructor dels VMs.

**Dependència**: R0 ✅ fet. La factory ja és qui construeix els VMs sense arguments (`Get<T>()`, des de l'scope del diàleg); falta la variant amb arguments de runtime.

**Feina concreta que R0 deixa preparada**:
1. Afegir `TWindow Get<TWindow>(params object[] vmArgs)` a `IWindowFactory`, implementat amb
   `ActivatorUtilities.CreateInstance(scope.ServiceProvider, WindowFactory.ViewModelTypeFor(typeof(TWindow)), vmArgs)`.
   Compte amb els `int?` (`ActuacioSetViewModel(bool, int?)`): cal treure'ls dels constructors abans, o passar-hi un tipus explícit.
2. Migrar els 16 `GetWith<XSetWindow>(new XSetViewModel(modeLookup: true))` a la nova sobrecàrrega.
3. Els `GetWith<XUpdateWindow>(interaction.Input)` són un cas diferent: el VM el construeix el `*SetViewModel`. O bé el VM pare rep una `Func<int, XUpdateViewModel>` injectada, o bé la `Interaction` passa a portar l'`id` en comptes del VM sencer (més net: el VM pare deixa de construir VMs).
4. Només llavors l'scope per diàleg de R0.3 comença a alliberar serveis de debò.

**Trampes**:
- Trencarà els 13 `<Design.DataContext>` → resoldre B3 primer (migrar a `x:DataType` + `x:CompileBindings="True"`).
- El filtre de registre de ViewModels a `DI/Injection.cs` (§R0.1) mira *«tots els paràmetres tenen valor per defecte»*. Quan els VMs rebin serveis pel constructor deixarà de valer: caldrà canviar-lo per *«tots els paràmetres són resolubles pel contenidor»*.
- Els `{…}RowViewModel` es creen en bucle dins dels `*SetViewModel`; caldrà un `Func<TDto, bool, TRowVm>` injectat, o passar la `IServiceFactory` avall.
- Abast: 2 projectes, ~30 fitxers. **Fer-ho en un commit separat.**

---

## R2 — Subscripcions imbricades no alliberades (bug real)

Patró repetit a **totes** les Create/Update windows:

```csharp
disposables(
    this.WhenAnyValue(x => x.ViewModel)
        .Subscribe(vm => vm!.SubmitCommand.Subscribe(CloseIfSaved))   // ← mai es fa dispose
);
```

Només es registra la subscripció **exterior**. La interior — i cada `RegisterHandler`, que retorna `IDisposable` — queda viva. Si la finestra es reactiva, `CloseIfSaved` s'executa N vegades i els handlers d'`Interaction` s'acumulen.

**Correcció**:
```csharp
this.WhenAnyValue(x => x.ViewModel)
    .Where(vm => vm is not null)
    .Subscribe(vm => vm!.SubmitCommand.Subscribe(CloseIfSaved).DisposeWith(d))
    .DisposeWith(d);
```

**Cas pitjor**: `AlumneInformeViewerWindow.axaml.cs:35` — subscriu dins del constructor, sense `WhenActivated` ni disposal, i amb `async void` a `Opened`.

**Cost baix, impacte alt.** Si es fa després de R3, només cal arreglar-ho en un lloc.

---

## R3 — Eliminar el boilerplate de diàlegs (~1.000 de 2.488 línies)

Duplicació mesurada:

| Grup | Fitxers idèntics | Línies |
|---|---|---|
| `{Centre,CursAcademic,Etapa,TipusActuacio}CreateWindow.axaml.cs` | 4, byte a byte tret del tipus | 45 c/u |
| `{…}UpdateWindow.axaml.cs` | 4 | 45 c/u |
| `{…}SetWindow.axaml.cs` | 6 | 50 c/u |
| `{…}RowUserCtrl.axaml.cs` | 4 | 65 c/u |
| Blocs de lookup a `Actuacio{Create,Update}` + `Alumne{Create,Update}` | 16 blocs gairebé iguals | ~180 total |
| `GetWindow() => (Window)this.VisualRoot!` | 10 còpies | — |
| `ObraFileExplorer()` amb `Process.Start` | 3 còpies (`AlumneRowUserCtrl`, `UtilitatsWindow`, `AlumneInformeViewerWindow`) | — |

**Solució**:
- Classes base genèriques: `EntityCreateWindow<TVm, TDto>`, `EntityUpdateWindow<TVm, TDto>`, `EntitySetWindow<TVm, TCreateVm, TDto>`, `EntityRowUserCtrl<TVm, TDto>`.
  L'`x:Class` de l'AXAML admet perfectament una classe base genèrica tancada.
- Helper de lookups sobre `IWindowFactory` (ja disponible des de R0):
  ```csharp
  d(this.RegisterLookup<AlumneSetWindow>(vm.ShowAlumneLookup, _windows));
  ```
  Els 16 blocs passen a 16 línies.
- Les classes base han de dur el paràmetre `IWindowFactory` al constructor i propagar-lo
  (`protected EntitySetWindow(IWindowFactory windows)`). Per als `EntityRowUserCtrl<…>`,
  posar-hi el constructor pont sobre `App.Services` **una sola vegada** i esborrar-lo dels 6 fitxers.
- Extensions: `TopLevelExtensions.GetOwnerWindow()`, `FileExplorer.Open(saveResult)`.

⚠️ `Process.Start(… Verb = "open")` és específic de Windows. En centralitzar-lo, val la pena gestionar `open` (macOS) i `xdg-open` (Linux) o deixar-ho documentat.

**Sinergia**: R3 fa que R2 s'hagi d'arreglar en 4 classes base en comptes de 20 fitxers. **Fer R3 abans que R2.**

---

## R4 — Treure la navegació del code-behind de `MainWindow`

`MainWindow.axaml.cs` té 8 handlers `X_OnClick` idèntics:
```csharp
private void Centre_OnClick(object? s, RoutedEventArgs e)
    => new CentreSetWindow { DataContext = new CentreSetViewModel() }.ShowDialog(this);
```
més 3 `RegisterShowXDialog` gairebé iguals.

**Objectiu**: comandes a `AppStatusViewModel` + `IWindowFactory`, i `Command=` a l'AXAML en comptes de `Click=`.

També: `try { } catch { }` buit a `DrawerSelectionChanged` (`MainWindow.axaml.cs:150`) que amaga errors — o es documenta què s'espera empassar, o desapareix.

---

## R5 — Codi mort (verificat)

| Element | Evidència |
|---|---|
| `ViewLocator.cs` | Cap referència. **No està registrat a `App.axaml`.** |
| `Converters/StringDateConverter.cs` | Zero referències a cap `.axaml`. Existeix una còpia viva a `UI.ER.ViewModels/Services/`. |
| `public OperationResult<T> Result { get; set; }` | Present a 7 finestres Create/Update. **Mai llegida ni escrita** (0 coincidències de `.Result`). |
| `UI.ER.AvaloniaUI.csproj` | `<AvaloniaResource Include="Assets\**" />` duplicat; `<Folder Include="Models\" />` apunta a una carpeta inexistent. |
| `using` no utilitzats | Desenes: `System.Linq`, `System.Threading.Tasks`, l'àlies `Dtoo` en fitxers que no el fan servir. |

Neteja gratuïta, sense risc. Es pot fer en qualsevol moment.

---

## R6 — Deriva de disseny i colors literals

- **`CentreSetWindow.axaml` s'ha quedat enrere**: encara fa servir el disseny antic (`DockPanel` + botó `DesarCss`), mentre `Etapa/CursAcademic/TipusActuacio` ja tenen el nou (`Border` de filtres + `material:FloatingButton`). Unificar.
- **95 colors literals** a l'AXAML (`#F5F7FA`, `#E3F2FD`, `#1565C0`, `#D32F2F`…) en comptes de recursos del `MaterialTheme`. Impedeix qualsevol canvi a tema fosc.
- **`Helpers/ConfirmationDialog.cs`** construeix la UI en C# amb colors literals, i el botó afirmatiu diu sempre *"Sí, esborrar"* independentment del missatge. Convertir-lo en AXAML parametritzat.

---

## 3. Ordre recomanat

```
R0  IWindowFactory + comprensió    ✅ FET
 │
R5  neteja de codi mort            ← sense risc, redueix soroll per a la resta
 │
R3  classes base + helpers         ← consumeix IWindowFactory; -1.000 línies
 │
R2  disposal de subscripcions      ← ja només a les 4 classes base
 │
R4  navegació de MainWindow        ← consumeix IWindowFactory
 │
R1  eliminar SuperContext          ← commit separat, travessa 2 projectes; cal B3 abans
 │
R7  BusinessLayer per comprensió   ← natural just després de R1
 │
R6  unificació visual              ← independent, es pot paral·lelitzar
```

> R5 s'ha desplaçat per darrere de R0 perquè R0 ja estava fet quan es va escriure aquest ordre.
> R5 continua sent el següent pas recomanat: és el de menys risc i neteja soroll per a R3.

---

## 4. Convencions per a l'agent

- **Idioma**: comentaris de codi, missatges d'error i textos d'UI en **català**. Identificadors en anglès o català segons el que ja hi hagi al fitxer.
- **Un refactor, un commit.** No barrejar R0 amb R1.
- **No introduir dependències noves** sense preguntar. L'stack actual és: Avalonia 11.3.11, ReactiveUI.Avalonia, Material.Avalonia, Material.Icons.Avalonia, Serilog.Sinks.File.
- **`dotnet build` ha de quedar net** després de cada pas. No hi ha tests d'UI: cada canvi estructural es valida obrint i tancant el diàleg afectat.
- **No tocar** `BusinessLayer`, `DataLayer`, `DataModels` ni les migracions durant R0/R2/R3/R4. R1 i R7 sí que hi entren.
- **`dotnet test UI.ER.AvaloniaUI.Test` ha de quedar verd després de cada pas.** Si un refactor canvia una invariant a consciència (p. ex. R3 introdueix classes base i el nombre de finestres es manté però els constructors canvien), s'actualitza el test amb el canvi, mai després.
- **Vistes noves**: no s'instancien amb `new`. Registrar-les no cal (l'escaneig les agafa soles), però han de complir la convenció de noms o portar `[ViewModel(typeof(...))]`, altrament l'aplicació no arrenca.
- Els fitxers `.axaml` i `.axaml.cs` van sempre junts: si es canvia l'`x:Class` o la classe base, revisar-ne els dos.

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

# UI.ER.AvaloniaUI — arquitectura de la capa de presentació

Aquest document descriu **com està construïda la capa de presentació** i quines regles no s'han
de trencar. Per a l'arquitectura de la resta de la solució (entitats, DTOs, projeccions,
BusinessLayer, EF Core) i per a la guia pas a pas de com afegir un CRUD nou, veure
[`../agents.md`](../agents.md).

Stack: **Avalonia 11.3.11** · ReactiveUI.Avalonia · Material.Avalonia · Material.Icons.Avalonia ·
Microsoft.Extensions.DependencyInjection · Serilog.Sinks.File · .NET 10.

---

## 1. Les dues meitats de la UI

| Projecte | Què hi ha | Volum |
|---|---|---|
| `UI.ER.ViewModels` | ViewModels (ReactiveUI), contractes de diàleg, `ServiceFactory` | 5.042 línies de `.cs` |
| `UI.ER.AvaloniaUI` | Vistes AXAML + code-behind, classes base, helpers, controls, paleta, DI | 2.089 línies de `.cs` · 34 AXAML |

La separació és estricta: **cap ViewModel referencia Avalonia**. Quan un ViewModel necessita que
passi alguna cosa a la pantalla —obrir un diàleg, demanar una confirmació— ho demana amb una
`Interaction<TEntrada, TSortida>` i és la vista qui decideix quina finestra l'atén (§5).

**Inventari**: 24 `Window` (`MainWindow`, `{Actuacio,Alumne,Centre,CursAcademic,Etapa,TipusActuacio}{Create,Update,Set}Window`,
`UtilitatsWindow`, `DadesUsuariWindow`, `CopiaDeSeguretatWindow`, `AlumneInformeViewerWindow`, `ConfirmacioWindow`) i 9 `UserControl`
(els 6 `*RowUserCtrl` més `DateInput`, `LookupInput` i `IndicadorCarrega`).
30 ViewModels, 29 dels quals reben serveis.

---

## 2. Composition root

Tot el contenidor es construeix en un sol lloc, `App.OnFrameworkInitializationCompleted()`:

```csharp
_services = new ServiceCollection()
    .DataLayerConfigureServices()        // DbContextFactory
    .BusinessLayerConfigureServices()    // les 30 operacions de BL
    .UIConfigureServices()               // vistes, ViewModels, IWindowFactory
    .BuildServiceProvider()
    .MigraBaseDeDades();                 // ← ja amb el contenidor definitiu

desktop.MainWindow = _services.GetRequiredService<IWindowFactory>().Get<MainWindow>();
```

**Els `*ConfigureServices` només registren.** Aplicar les migracions és una passa a part,
`MigraBaseDeDades()`, encadenada després del `BuildServiceProvider()`. Feta des de dins del
registre —com estava— caldria un `BuildServiceProvider()` intermedi per arribar a la
`IDbContextFactory`: un segon contenidor, amb els seus propis singletons i el seu propi pool de
connexions, que es llença tot seguit.

Els altres dos executables (`ImportData`, `CreateDemoData`) es munten igual, sense
`UIConfigureServices()`.

### Tot es registra per comprensió

**Cap dels tres mètodes de registre conté una llista escrita a mà.** És deliberat: una llista
deriva, un escaneig no.

| Registre | Regla | Cicle de vida |
|---|---|---|
| Finestres (`DI/Injection.cs`) | tot tipus no abstracte assignable a `Window` de l'assembly | `Transient` |
| ViewModels (`DI/Injection.cs`) | tot `ViewModelBase` amb **tots els paràmetres del constructor resolubles** (registrats o amb valor per defecte) → en surten 16 | `Transient` |
| `IServiceFactory` (`DI/Injection.cs`) | fix | `Scoped` |
| `IWindowFactory` (`DI/Injection.cs`) | fix | `Singleton` |
| Operacions de BL (`BusinessLayer/DI/Injection.cs`) | cada `IXxx` del namespace `BusinessLayer.Abstract.Services` aparellada amb `BusinessLayer.Services.Xxx` **pel nom** → 30 | `Transient` |
| `INotificadorDeCanvis` (`BusinessLayer/DI/Injection.cs`) | fix | `Singleton` |
| `IDadesDeLusuari` (`BusinessLayer/DI/Injection.cs`) | fix | `Singleton` |

El filtre dels ViewModels deixa fora, sols, els 13 que necessiten un argument de runtime
(els 6 `{…}UpdateViewModel(int id)`, els 6 `{…}RowViewModel(DTO)` i `AlumneInformeViewerViewModel(int alumneId)`).
Aquests passen sempre per `GetWith` o per `Get` amb arguments.

El registre de les operacions no passa la classe d'implementació sinó una **fàbrica**: crea
la instància amb `ActivatorUtilities` i li assigna el bus de canvis per propietat. Les
operacions són `Transient` i el bus `Singleton`, i posar-l'hi pel constructor voldria dir
tocar-ne 19; així és un sol lloc, i una operació nova hereta l'emissió pel sol fet d'heretar
la seva classe base (§5.1).

> ⚠️ L'ordre importa: l'`IServiceFactory` s'ha de registrar **abans** de l'escaneig dels
> ViewModels, perquè és la dependència que tots ells demanen. Si es registrés després, el filtre
> no en sabria res i no es registraria cap ViewModel. El mateix per a l'`INotificadorDeCanvis`,
> que registra el BusinessLayer i que a la composition root ja hi va primer. Ho vigilen dos tests.

### Fallada ràpida a l'arrencada

`ValidaConvencioVistaViewModel()` recorre **totes** les finestres i comprova que cadascuna resol
el seu ViewModel; l'equivalent al BusinessLayer comprova que cada contracte té implementació.
Els dos acumulen **tots** els errors i llancen un sol cop amb la llista sencera. Afegir una
finestra fora de convenció, o una interfície de servei sense implementació, peta a l'arrencada
i no dins d'un diàleg tres clics més tard.

---

## 3. De la vista al ViewModel: `IWindowFactory`

**Cap fitxer `.cs` de la UI fa `new XWindow(`.** Les finestres surten sempre de la fàbrica:

```csharp
TWindow Get<TWindow>(params object[] vmArgs) where TWindow : Window;
TWindow GetWith<TWindow>(ViewModelBase dataContext) where TWindow : Window;
```

- **`Get<T>()`** — la fàbrica resol la finestra *i* el ViewModel. Amb `vmArgs` els barreja amb
  les dependències del contenidor via `ActivatorUtilities` (aparellament **per tipus**, no per
  posició). És el camí de la navegació del menú i dels 16 lookups (`Get<AlumneSetWindow>(modeLookup)`).
- **`GetWith<T>(vm)`** — el ViewModel arriba construït des de fora. És el cas de les
  `Interaction`, on el ViewModel pare construeix el fill perquè ja té la `IServiceFactory` per
  passar-l'hi.

`GetWith` **no** difereix de `Get` en el *tipus* del ViewModel, només en *qui el construeix*: per
això comprova en runtime que el `dataContext` sigui el ViewModel que la convenció assigna a
`TWindow` i llança abans d'obrir el diàleg. Sense la comprovació, un desaparellament es
manifestaria com un diàleg amb els bindings buits i cap error.

### Convenció Vista → ViewModel

Treure el sufix `Window` o `UserCtrl`, afegir `ViewModel`, buscar-lo a
`UI.ER.ViewModels.ViewModels`. L'única excepció és `MainWindow`, marcada amb
`[ViewModel(typeof(AppStatusViewModel))]`. L'atribut té prioritat sobre la convenció; no hi ha
cap `switch` de casos especials enlloc.

### Un scope per diàleg

`Build<TWindow>` obre un `IServiceScope`, hi resol la finestra i el ViewModel, i el disposa a
`Closed`. Com que l'`IServiceFactory` és `Scoped`, les operacions de BL —`Transient` i
`IDisposable`— queden apuntades a l'scope del diàleg i s'alliberen en tancar-lo.

### El constructor pont

17 vistes necessiten la fàbrica. Totes segueixen el mateix patró de dos constructors encadenats:

```csharp
public CentreSetWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }
public CentreSetWindow(IWindowFactory windows) : base(windows) => InitializeComponent();
```

El constructor sense paràmetres **no és opcional**: els `*RowUserCtrl` els instancia el
`ListBox.ItemTemplate` des de l'AXAML i les finestres sense ell fan que el compilador d'Avalonia
emeti `AVLN3001`. Encadenar-los fa que no importi quina via s'agafi. `App.Services` és el provider
arrel, `public static` **exclusivament** per a aquests 17 punts; és l'únic service locator que
queda a la capa de vistes i no és reduïble mentre l'AXAML pugui instanciar vistes pel seu compte
(els constructors no s'hereten en C#).

---

## 4. Del ViewModel al BusinessLayer: `IServiceFactory`

Cap ViewModel resol serveis pel seu compte. Reben `IServiceFactory` com a **primer** paràmetre del
constructor i el guarden a `_serveis`; les 48 crides al BL tenen totes la mateixa forma:

```csharp
using var bl = _serveis.GetBLOperation<ICentreCreate>();
var result = bl.Create(parms);
```

**Per què una fàbrica i no els serveis concrets.** Les operacions de BL són `Transient` i
`IBLOperation : IDisposable`, i es consumeixen amb `using var` a cada crida. Una instància
injectada quedaria disposada després del primer ús i la segona obertura del diàleg petaria.

**Per què `Scoped`.** MS.DI apunta els transitoris `IDisposable` a l'scope que els ha creat. Amb
la fàbrica resolta des de l'arrel, la llista de disposables creixia amb un objecte per cada
operació que s'hagués fet mai i només es buidava en tancar l'aplicació. Amb `Scoped`, la llista
és la de l'scope del diàleg i mor amb ell. Dos tests ho fixen, un dels quals comprova que
`BuildServiceProvider(validateScopes: true)` **es nega** a resoldre-la des de l'arrel.

`IServiceFactory` és **el port únic dels ViewModels cap al BusinessLayer**, amb tres cares:
`T GetBLOperation<T>() where T : IBLOperation` per demanar-li operacions,
`INotificadorDeCanvis Canvis` per escoltar-ne els canvis (§5) i `IDadesDeLusuari DadesUsuari`
per saber qui fa servir el programa. Segueix sense ser un `IServiceProvider` disfressat: des
d'aquí no s'arriba a cap altre servei.

Les dues propietats hi són pel mateix motiu: el genèric de `GetBLOperation<T>()` està acotat a
`IBLOperation` i no les pot tornar, i demanar-les pel constructor del ViewModel el faria
semblar un ViewModel amb arguments de runtime —la col·lecció de `FabricaDeServeisTest` és
`UIConfigureServices()` tota sola, sense BusinessLayer— i el test exigiria que el primer
paràmetre fos l'`IServiceFactory`.

I hi ha una raó pràctica que val per a totes dues: hi ha **27 punts** on un ViewModel en
construeix un altre amb `new`, i qualsevol paràmetre de constructor nou s'hauria d'anar
propagant amunt i avall de tota la jerarquia. La fàbrica ja hi arriba a tots.

---

## 5. Navegació i diàlegs

El ViewModel declara una comanda i una `Interaction`; la vista diu quina finestra l'atén. Cinc
helpers a `Helpers/DialegExtensions.cs` cobreixen tots els casos:

| Helper | Quan | Com obté la finestra |
|---|---|---|
| `RegistraNavegacio<TWindow>` | entrada de menú, botó del taulell | `Get<T>()` |
| `RegistraDialeg<TWindow, TEntrada[, TSortida]>` | el ViewModel del diàleg ve de la `Interaction` | `GetWith<T>(input)` |
| `RegistraLookup<TSetWindow>` | els 16 lookups d'`Alumne`/`Actuacio` | `Get<T>(modeLookup: true)` |
| `RegistraConfirmacio` | confirmació d'una acció irreversible | `GetWith<ConfirmacioWindow>(…)` |

**Cap vista navega des d'un handler de `Click`.** A l'AXAML és `Command="{Binding CentreSetCommand}"`,
i al code-behind una línia dins del `WhenActivated`. Els handlers que queden a `MainWindow`
no naveguen: uns són estat de la pròpia finestra (el calaix lateral, el `Carousel`, la snackbar)
i els altres són ordres a l'aplicació sencera —canviar de tema, sortir—, que no tenen finestra de
destí. Un test ho vigila, amb la llista dels que s'accepten escrita a `NavegacioTest`.

### Els porticons d'arrencada

Dues finestres es poden obrir soles, **en aquest ordre i un sol cop per sessió**, des
d'`AppStatusViewModel.ObreElsPorticonsDArrencada()`:

| # | Finestra | Quan surt | Com se'n surt |
|---|---|---|---|
| 1 | `DadesUsuariWindow` | `_serveis.DadesUsuari.EstaInformat` és fals | Es tanca; el programa funciona igual |
| 2 | `CopiaDeSeguretatWindow` | `ICopiaDeSeguretat.CalFerCopia()` diu que sí: fa més de dues setmanes de l'última còpia **i** hi ha actuacions noves | Botó «Ara no», o la creu |

**Encadenats, no en paral·lel**: el segon es llança des del `Subscribe` del primer, quan el
seu diàleg s'ha tancat. Dos `ShowDialog` alhora es taparien l'un a l'altre.

El *si cal* de la còpia no és del ViewModel: la regla viu a `ICopiaDeSeguretat.CalFerCopia()`,
i des d'allà la comparteixen el taulell —que decideix obrir la finestra— i la finestra
mateixa, que amb la mateixa resposta pinta la pancarta i el botó «Ara no». Si fossin dues
còpies de la regla, la finestra s'obriria dient que no cal fer res.

Cap dels dos bloqueja. Una còpia de seguretat que impedeixi treballar el dia que el llapis no
hi és fa més mal que bé; la proposta torna a sortir la propera arrencada.

Qui els **dispara** és `MainWindow.Registra(d)`, just després de registrar la navegació, amb
`Dispatcher.UIThread.Post(vm.ObreElsPorticonsDArrencada, DispatcherPriority.Background)`. El
*si cal* i el *què s'obre* continuen sent del ViewModel; la vista només diu «ja tinc els
handlers posats i la finestra mostrada».

> ⚠️ És el punt més delicat de tot el camí, i el primer a mirar si el diàleg no surt.
> Llançar-lo des del `WhenActivated` del ViewModel **no funciona**, ni tan sols diferit amb
> `RxApp.MainThreadScheduler.Schedule`: l'`AvaloniaScheduler` executa **en línia** les
> accions sense retard quan ja s'és al fil d'UI, i la `Interaction` arriba abans que els
> `RegisterHandler` de la vista —que es registren a la mateixa passada d'activació— i peta
> amb `UnhandledInteractionException`, que a l'arrencada tomba l'aplicació. `Post`, en canvi,
> no s'executa mai en línia. I no pot anar a `App.OnFrameworkInitializationCompleted()`
> perquè un `ShowDialog` necessita un propietari ja mostrat.

### El bus de canvis de domini

Les finestres de lookup tenen CRUD complet i se'n poden tenir vàries obertes alhora: **una pot
modificar dades que una altra ja té pintades**. El que queda obsolet no és tant l'entitat
editada com els **camps derivats i les etiquetes desnormalitzades** que els DTO de sortida
porten a dins —`Alumne.NombreActuacions`, `CentreAmbActuacions.TotalActuacions`, el nom del
centre dins d'una fila d'actuació.

Ho resol un **bus**: `INotificadorDeCanvis`, `Singleton`, que emet des de les cinc classes base
d'escriptura del BusinessLayer i al qual les llistes s'hi subscriuen.

```csharp
public sealed record CanviDeDomini(MenaDeCanvi Mena, IReadOnlySet<Referencia> Afectats);
public readonly record struct Referencia(Entitat Entitat, int Id);
```

L'aparellament entre el que ha canviat i el que s'ha de refrescar **no és un mapa de
dependències escrit a mà**. És una sola funció de convenció (`Referencies.De`) aplicada als dos
costats:

> `Referencies(dto) = { el propi dto } ∪ { tota propietat IIdEtiquetaDescripcio que exposa }`

L'emissor l'aplica al DTO que acaba d'escriure; el receptor, al que té pintat. **Una llista es
refresca quan els dos conjunts s'intersequen.** Funciona perquè les projeccions
(`DTO.Projections`) passen els models d'EF sencers al constructor del DTO, i per tant
`Dtoo.Actuacio` ja porta a dins l'alumne, el tipus, el curs, el centre i l'etapa amb els seus
`Id`. La correspondència tipus → `Entitat` és **pel nom**, pujant per `BaseType`: així val alhora
per al DTO, per al model i per a `CentreAmbActuacions`, que resol pel seu base `Centre`.

| Peça | On |
|---|---|
| Contracte, `Referencies`, `CanviDeDomini` | `BusinessLayer.Abstract/Generic/` |
| Implementació | `BusinessLayer/Common/NotificadorDeCanvis.cs` |
| Emissió | `BLCreate`, `BLUpdate`, `BLDelete`, `BLActivaDesactiva`, `BLBatchOperation` |
| Adaptació a Rx | `NotificadorExtensions.ComObservable()` |
| Subscripció | `SetViewModelBase` i `AppStatusViewModel` |

Tres decisions que costen d'endevinar:

1. **El contracte és un `event`, no un `IObservable`.** Ni `BusinessLayer` ni
   `BusinessLayer.Abstract` referencien System.Reactive, i el bus no és motiu per fer-los-hi
   dependre. El costat UI l'adapta amb `Observable.FromEvent`.
2. **Qui se subscriu és el `*SetViewModel`, mai la fila.** Els `ListBox` virtualitzen: les
   vistes de fila es reciclen i es desactiven en fer scroll, i una subscripció viva a la fila
   perdria notificacions mentre està fora de pantalla.
3. **`BLUpdate` publica les referències del DTO previ i les del nou.** És l'única consulta que
   el bus afegeix, i és la que fa que moure una actuació de l'alumne #7 al #9 refresqui els dos
   comptadors.

`BLBatchOperation` no sap què ha tocat: publica `CanviDeDomini.Tot` i tothom es refresca.

### Refresc silenciós

El refresc **no** és rellegir fila a fila —serien fins a 200 consultes—: és repetir la mateixa
consulta de la llista i **pedaçar les files existents casant per `Id`**, sense `Clear()`, sense
tocar `Loading` i sense reconstruir la col·lecció, de manera que no es perd ni la posició de
scroll ni la selecció. Al davant hi va un `Throttle(300 ms)` per no repetir-ho en ràfegues.

Les files que ja no surten a la consulta **es treuen**. Les que hi apareixen de nou **no
s'afegeixen**: podrien no complir el filtre de qui mira, i fer-les aparèixer li mouria el que
està llegint. Els diàlegs d'edició oberts tampoc escolten res —no se't pot sobreescriure el que
estàs escrivint.

La finestra que ha fet el canvi també rep la notificació i es refresca redundantment. És una
consulta de més i és inofensiu; resoldre-ho amb un token d'origen no val la pena fins que no es
demostri que molesta.

---

## 6. Com s'escriu una vista

Tres classes base a `Pages/Base/` absorbeixen el boilerplate. Una vista típica són 18 línies:

```csharp
public partial class CentreCreateWindow : EntityEditWindow<CentreCreateViewModel, Dtoo.Centre>
{
    public CentreCreateWindow() => InitializeComponent();
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
```

| Classe base | Per a | Què fa sola |
|---|---|---|
| `EntityEditWindow<TVm, TDto>` | les 12 `{Create,Update}Window` | tanca la finestra retornant el DTO quan `SubmitCommand` acaba bé |
| `EntitySetWindow<TVm, TCreateVm, TCreateWindow, TDto>` | les 6 `*SetWindow` | el diàleg d'alta |
| `EntityRowUserCtrl<TVm, TUpdateVm, TUpdateWindow, TResultat, TDto>` | els 6 `*RowUserCtrl` | el diàleg d'edició des de la fila |

I una al costat dels ViewModels, `UI.ER.ViewModels/ViewModels/Base/`:

| Classe base | Per a | Què fa sola |
|---|---|---|
| `SetViewModelBase<TRow, TDto>` | els 6 `*SetViewModel` | la col·lecció de files, `Loading`/`PaginatedMsg`/`BrokenRules`, el bucle consulta → files, i la subscripció al bus amb el refresc silenciós (§5) |

Cada llista només hi posa la seva `Consulta()` —amb els paràmetres dels seus filtres— i el seu
`CreaFila(dto)`, que encapsula el `new XxxRowViewModel(…)` (tenen signatures diferents: la fila
d'alumne vol el curs actiu, la de curs acadèmic vol la col·lecció sencera). Qui necessiti dades
que no vinguin de la consulta sobreescriu `AbansDeCrearFiles()`.

Perquè la classe base pugui pedaçar una fila li cal saber-ne tres coses: és el contracte
`IFilaDeLlista<TDto>` (`Id`, `ReferenciesPintades` i `Actualitza(dto)`), del qual deriva
`IRowViewModel<…>`.

Perquè una classe base genèrica pugui cridar `vm.SubmitCommand` cal una restricció que ho
garanteixi: són les quatre interfícies de
`UI.ER.ViewModels/ViewModels/Contracts/DialegContracts.cs` (`ISubmitViewModel<TDto>`,
`ISetViewModel<…>`, `IFilaDeLlista<TDto>`, `IRowViewModel<…>`). Són **purament declaratives**:
cap ViewModel canvia de comportament per implementar-les.

Qui necessita més coses sobreescriu `Register(CompositeDisposable d)`, crida `base.Register(d)` i
hi afegeix les seves subscripcions.

### Activació del ViewModel

`ViewModelBase` implementa `IActivatableViewModel`. El `WhenActivated` de la vista activa també
l'`Activator` del seu ViewModel, i per tant una subscripció que el ViewModel faci al seu
constructor mor en tancar-se la finestra **sense tocar cap vista** —i tant si el ViewModel l'ha
resolt el contenidor com si el pare l'ha fet amb `new`, que és el cas dels que passen per
`Interaction`. És el que fa que la subscripció al bus (§5) no s'acumuli en obrir i tancar la
mateixa finestra.

### Subscripcions i disposal

Totes les subscripcions van dins de `WhenActivated` i **totes** acaben amb `.DisposeWith(dd)`,
inclosos els `RegisterHandler` de les `Interaction`, que també retornen `IDisposable`. El
`CompositeDisposable` es propaga cap endins amb `PerCadaViewModel`:

```csharp
protected void PerCadaViewModel(CompositeDisposable d, Action<TVm, CompositeDisposable> accio)
    => this.WhenAnyValue(x => x.ViewModel)
           .Where(vm => vm is not null)
           .Subscribe(vm => accio(vm!, d))
           .DisposeWith(d);          // ← també l'exterior
```

Registrar només la subscripció exterior fa que, en reactivar-se la vista, els handlers
s'acumulin i el tancament automàtic s'executi N vegades.

> ⚠️ `DisposeWith` viu a `System.Reactive.Disposables.Fluent` (System.Reactive 6.1), **no** a
> `System.Reactive.Disposables`. Calen els dos `using` al mateix fitxer.

---

## 7. L'AXAML

### Compiled bindings a tot arreu

Els **33 fitxers** porten `x:CompileBindings="True"`; cap dels **349 `{Binding …}`** es resol per
reflexió. Els que lliguen al `DataContext` declaren `x:DataType`; els tres controls amb
`StyledProperty` no en tenen (no tenen ViewModel) però els seus bindings han d'apuntar al **tipus
concret**:

```xml
<!-- compila, però contra Avalonia.Controls.UserControl: no comprova res -->
Text="{Binding DateText, RelativeSource={RelativeSource AncestorType=UserControl}}"
<!-- ara sí -->
Text="{Binding DateText, RelativeSource={RelativeSource AncestorType=controls:DateInput}}"
```

Tres coses que costen d'endevinar:

1. **El `clr-namespace` necessita el `;assembly=UI.ER.ViewModels`.** Sense ell `x:DataType` no
   resol i el compilador escup `AVLN2000: Unable to resolve type`.
2. **`dotnet build` incremental no recompila l'AXAML.** Qualsevol canvi a un `.axaml` s'ha de
   verificar amb `--no-incremental`; si no, els `AVLN####` no surten.
3. **No hi ha `<Design.DataContext>` enlloc.** Instanciava el ViewModel, i per tant li exigia un
   constructor sense paràmetres — incompatible amb la injecció. Compila igualment, així que és un
   test qui ho guarda, no el compilador.

Dins d'un `DataTemplate` sense `x:DataType` propi el tipus **s'infereix de l'`ItemsSource`**: els
bindings de les plantilles també es comproven.

### Colors: la paleta semàntica

`Themes/Paleta.axaml` és un `ResourceDictionary` amb `ThemeDictionaries` (`Light` i `Dark`), 23
pinzells per tema. **Cap color s'escriu a pèl a cap altre fitxer**, ni `#RRGGBB` ni amb nom
(`Red`, `Gray`).

> L'única excepció viva és el `BoxShadow` de `Border.Fitxa` a `App.axaml`, on el color va
> **enmig** d'un valor compost (`Value="5 5 10 2 #40000000"`). `DissenyTest` només detecta els
> valors que són *exactament* un color (`"#…"`), així que aquest se li escapa. És una ombra amb
> alfa, no un color de la paleta, i per això es deixa estar.

Les claus són **semàntiques, no descriptives** — `WarningBrush`, no `Taronja` — amb tres sufixos:
`XxxBrush` (primer pla), `XxxContainerBrush` (fons del bloc) i `XxxBorderBrush` (vora). Set
famílies: neutres, `Info`, `Success`, `Warning`, `Danger`, `Accent` i `Header`.

**Canviar de tema** és moure `RequestedThemeVariant` (`App.axaml`), i prou: el `MaterialTheme`
va declarat amb `BaseTheme="Inherit"` i el segueix tot sol. El valor de l'AXAML és només el tema
d'arrencada; `Helpers/Tema.cs` el commuta en calent des de l'entrada de menú «Canvia el tema».

> ⚠️ No es deixa `RequestedThemeVariant` **sense fixar**: Avalonia passaria a seguir el tema del
> sistema operatiu i l'aplicació canviaria de cara sense que ningú ho hagi demanat.

Els contrastos de la taula `Dark` estan per sobre de 4.5:1 a totes les parelles
primer-pla/contenidor; l'únic que hi baixa és `TextTertiaryBrush` (3.7–4.5:1 segons la
superfície), que és text auxiliar. La taula `Light` hi va més justa: `TextTertiaryBrush` es queda
a ~2.5:1 i `WarningBrush` sobre el seu contenidor a 3.5:1.

### Estils compartits

El que estava copiat va a `App.axaml` com a classe i la vista només diu quina vol:
`Border.Fitxa`, `.BarraFiltres`, `.Xip`, `.Seccio{,.Ok,.Avis,.Perill}`, `.Apareix`/`.Desapareix`,
`material:FloatingButton.Afegir` i `.Desar`. El text del FAB va per `ContentTemplate`, de manera
que cada finestra només escriu `Content="Nova etapa"`.

---

## 8. Els tests

Cap test obre una finestra ni toca la base de dades: són **tests estructurals** sobre reflexió i
sobre el codi font. Corren en ~80 ms.

```
dotnet test UI.ER.AvaloniaUI.Test      → 60/60
dotnet test BusinessLayer.Integration.Test → 56/56
```

| Fitxer | Què fixa |
|---|---|
| `Vistes.cs` | punt únic des d'on s'enumeren vistes i ViewModels reals — **cap test escriu llistes de tipus a mà**, una vista nova entra sola a tots |
| `ConvencioVistaViewModelTest` | cada finestra resol el seu ViewModel; l'atribut guanya a la convenció |
| `RegistreDITest` | cicles de vida; es registren **exactament** els ViewModels construïbles sense arguments |
| `ConstructorsDeVistaTest` | tota vista té constructor buit i el constructor pont **encadena de debò** (inspecció de l'IL) |
| `WindowFactoryTest` | `GetWith` rebutja un ViewModel desaparellat; `Get` barreja arguments i dependències |
| `FabricaDeServeisTest` | l'`IServiceFactory` és `Scoped`, no surt de l'arrel, i cap ViewModel torna a guardar serveis en un camp estàtic |
| `ClassesBaseTest` | que no torni el boilerplate que absorbeixen les classes base, ni el bucle de càrrega que absorbeix `SetViewModelBase` |
| `ReferenciesTest` | la funció de convenció del bus troba **totes** les propietats `IIdEtiquetaDescripcio` de cada DTO de sortida. Escaneig, no llista |
| `EntitatTest` | cada valor de l'enum `Entitat` té DTO i model amb el mateix nom, i cap entitat del domini es queda fora de l'enum |
| `BusTest` | la subscripció d'una llista es dóna de baixa en desactivar-se; la regla «`Afectats` interseca les referències pintades»; les files recalculen les seves referències |
| `NavegacioTest` | cada llista és arribable des del taulell; `MainWindow` no navega amb handlers de `Click` |
| `DissenyTest` | cap color literal; els dos temes defineixen les mateixes claus; cap clau morta ni cap errata en un `DynamicResource` |
| `BindingsCompilatsTest` | els 33 AXAML declaren `x:CompileBindings`; cap `<Design.DataContext>` |
| `InjeccioTest` (BL) | cada contracte té la seva implementació per convenció, i les 30 es resolen de debò; el bus és `Singleton` i es registra abans |
| `DadesUsuariTest` (BL) | l'`Usuari.ini`: sense fitxer no es crea res, l'anada i tornada conserva accents i ela geminada, una dada invàlida no toca ni el disc ni la memòria, les claus alienes es conserven i un fitxer escombraria no impedeix arrencar |
| `EmissioTest` (BL) | les cinc classes base d'escriptura publiquen al bus i cap operació se salta l'emissió; alta, modificació, baixa i massiu emeten el que toca contra una base de dades de veritat |

`DissenyTest` i `BindingsCompilatsTest` escanegen el **codi font** (via `[CallerFilePath]`). És
deliberat: un color literal compila igual de bé que una clau de recurs, un `DynamicResource` que
no resol **no peta**, i un `<Design.DataContext>` tampoc — el que es vol vigilar és què s'escriu.

**Tots els tests estan verificats per mutació**: no n'hi ha prou que passin, han de posar-se
vermells quan toca. Un test que no es pugui verificar per mutació no es deixa al projecte.

---

## 9. Invariants — no trencar

1. **Els `Window` d'Avalonia no es poden reobrir després de `Close()`.** Qualsevol registre de
   vista ha de ser `Transient`. Un `Singleton` peta al segon `ShowDialog`.
2. **Els `*SetViewModel` disparen la càrrega de dades *dins* del constructor**
   (`WhenAnyValue(x => x.NomesActius).Subscribe(…)` emet immediatament). Per això `ModeLookup`
   **no** pot passar a ser `{ get; init; }`: ha de continuar sent argument del constructor.
3. **Cap ViewModel resol serveis pel seu compte** ni els guarda en un camp estàtic.
4. **Tota vista conserva un constructor `public` sense paràmetres** (§3).
5. **Cada vista amb `x:Class` conserva el seu `InitializeComponent()`.** `AvaloniaXamlLoader.Load(this)`
   es queda a la classe derivada, mai a la base: el compilador el reescriu a una crida directa al
   mètode generat només quan el troba dins del tipus que declara l'AXAML. Pujar-lo a la base
   compilaria igual, però passaria a resoldre's per reflexió.
6. **Els escanejos filtren `IsAbstract` i `IsGenericTypeDefinition`.** És el que manté les classes
   base fora del registre i dels tests d'inventari.
7. **Les vistes no fan `new` d'altres vistes.** Tot passa per `IWindowFactory`.
8. **Cap vista navega des d'un handler de `Click`.**
9. **Cap color s'escriu a pèl.**
10. **El tema es tria en un sol lloc: `RequestedThemeVariant`.** El `MaterialTheme` el segueix
    via `BaseTheme="Inherit"`; posar-hi `Light` o `Dark` a pèl torna a obrir la porta a què els
    dos valors se separin.
11. **Les llistes no es refresquen a mà: tot passa pel bus.** Ni un `ReLoadData()` en tancar un
    diàleg, ni un `Subject` entre fila i llista, ni una crida a `LoadData()` des d'una comanda de
    navegació — hi havia les tres coses i totes tres han desaparegut. Qui escriu publica; qui
    pinta escolta.

---

## 10. Convencions per afegir coses

**Una finestra nova**: no cal registrar-la (l'escaneig l'agafa sola), però ha de complir la
convenció de noms o portar `[ViewModel(typeof(...))]`; altrament l'aplicació no arrenca. Si obre
diàlegs, li calen els dos constructors encadenats. Si és una `{Create,Update,Set}Window` o un
`*RowUserCtrl`, ha d'heretar de la classe base corresponent — hi ha un test que ho comprova.

**Una operació de BusinessLayer nova**: crear `IXxx` a `BusinessLayer.Abstract/Services/` i `Xxx`
a `BusinessLayer/Services/`. Res més: el registre la troba pel nom i, si hereta d'una de les
classes base d'escriptura, l'emissió al bus li ve de franc. Una interfície sense implementació
peta a l'arrencada.

**Una entitat nova**: a més del CRUD, el seu nom ha d'entrar a l'enum `Entitat`
(`BusinessLayer.Abstract/Generic/CanviDeDomini.cs`) perquè les llistes que la pintin es
refresquin. `EntitatTest` ho vigila.

**Un color nou**: s'afegeix a les **dues** taules de tema de `Paleta.axaml`. Un pinzell que ningú
faci servir també fa fallar els tests.

**Un bloc de disseny que surti a més de dues vistes**: va a `App.axaml` com a classe.

**Un diàleg del sistema** (selector de carpetes, de fitxers): és d'Avalonia i el ViewModel no
el pot conèixer. Va per `Interaction<TEntrada, TSortida>` com la navegació, però la registra el
codi rere **la seva pròpia finestra**, no `MainWindow`: `CopiaDeSeguretatWindow` ho fa amb
`ShowTriaCarpetaDialog` i `StorageProvider.OpenFolderPickerAsync`. `NavegacioTest` només mira les
`Interaction` d'`AppStatusViewModel`, de manera que una d'aquestes no li demana cap comanda de
taulell.

**Idioma**: comentaris, missatges d'error i textos d'UI en **català**.

**Finals de línia**: són **barrejats fitxer a fitxer** (`App.axaml`, `Views/MainWindow.axaml`,
`App.axaml.cs` i uns quants ViewModels són CRLF; la resta LF) i alguns `.cs` porten BOM. Un
script de reescriptura els normalitza sense voler i converteix un canvi de dues línies en un diff
de 478: cal detectar el final de línia del fitxer i tornar-lo a escriure en binari.

---

## 11. Deutes oberts

Cap és un blocador; tots estan aquí perquè no s'oblidin.

| # | Deute | Per què |
|---|---|---|
| 1 | **Els diàlegs d'edició fan servir l'scope del ViewModel pare.** Obrir i tancar la fitxa d'un centre 20 vegades acumula les seves operacions a l'scope de la `CentreSetWindow`, no a la seva. | Segueix sent una millora estricta sobre el provider arrel. Tancar-ho vol dir que la `Interaction` porti l'`id` en comptes del ViewModel sencer, i això toca els tres contractes de diàleg i les tres classes base. La conseqüència principal —subscripcions que no moren amb el diàleg— la mitiga l'activació del ViewModel (§6). |
| 2 | **El quart clon de `PerCadaViewModel`**: `MainWindow` es fa el seu, perquè no hereta de cap classe base. | Extreure'l demanaria tipar-lo sobre `IViewFor<TVm>` i comprovar que `WhenAnyValue` continua resolent l'`ICreatesObservableForProperty` d'Avalonia — verificable només amb `Avalonia.Headless`, que avui no hi és. |
| 3 | **El tema fosc encara no s'ha mirat amb la pantalla al davant.** Ja és commutable des del menú i els contrastos calculats donen bé, però ningú n'ha vist les 23 finestres. | Les xifres no diuen res dels colors que venen del `MaterialTheme` ni de com queden les ombres i les vores sobre fons fosc. |
| 4 | **Els 17 `App.Services` dels constructors pont.** | No reduïbles mentre l'AXAML pugui instanciar vistes pel seu compte (§3). |

---

## 12. Compilar i provar

```bash
# Build complet. --no-incremental és obligatori si s'ha tocat cap .axaml:
# l'AXAML es compila a part i l'incremental no el recompila.
dotnet build eaprecull.sln --no-incremental

dotnet test UI.ER.AvaloniaUI.Test
dotnet test BusinessLayer.Integration.Test
```

Un build net ha de donar **0 errors i cap warning de C#, d'Avalonia ni dels analitzadors**. Els
`NU1903` que queden són vulnerabilitats de paquets de tercers, no codi d'aquest repositori.

**Cap test obre una finestra**, així que un canvi estructural es valida també obrint i tancant a
mà el diàleg afectat. El recorregut mínim després de tocar la UI:

- Les sis llistes des del menú: han de carregar dades i refrescar les xifres del taulell en tancar.
- Els 16 lookups d'`Actuacio{Create,Update}` i `Alumne{Create,Update}`: cada un ha d'obrir la
  llista en mode selecció i tornar l'element.
- Alta i modificació d'una entitat de cada tipus, amb `Ctrl+S`.
- Esborrar una actuació (valida `ConfirmacioWindow`; `Esc` ha de cancel·lar).
- L'expedient d'un alumne i la seva exportació a Word, i el pivot d'`Utilitats`.
- Obrir i tancar **el mateix diàleg tres vegades seguides** (invariant 1 + disposal d'scope + la
  baixa de la subscripció al bus).
- Amb **dues finestres obertes alhora** (§5): donar d'alta una actuació ha de pujar el comptador
  de la fila de l'alumne sense parpelleig i sense perdre la posició de scroll; canviar el nom
  d'un alumne des d'una fila d'actuació ha de canviar totes les seves files i la llista
  d'alumnes; reanomenar un centre només ha de tocar les files dels seus alumnes; esborrar una
  actuació ha de fer baixar el comptador del curs i treure la fila amb la seva animació;
  «Sincronitza alumnes per centre» d'`Utilitats` ha de refrescar totes les llistes obertes.
- El calaix lateral de `MainWindow`: les dues entrades seleccionables i el `Carousel` canviant de pàgina.
- «Canvia el tema» del menú, i tornar a passar per sobre de les vistes obertes.
- «Sortir» del menú: ha de tancar l'aplicació sencera, no només la finestra.
- `error.log` buit.

Per generar l'executable distribuïble, veure [`../README.md`](../README.md).

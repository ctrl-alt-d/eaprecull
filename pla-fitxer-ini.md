# Pla: fitxer `.ini` amb les dades de l'usuari

> Estat: **proposta**, pendent de confirmar les decisions de §12.
> Context d'arquitectura: `ARQUITECTURA.md` §6 (persistència), §7 (DI), §8 (presentació).

## 1. Què cal fer

- Un fitxer `.ini` al costat de la base de dades que desa **nom**, **cognoms** i
  **adreça xtec** de qui fa servir el programa.
- Si el fitxer no existeix o no està informat, en obrir el programa apareix la
  pantalla d'edició d'aquestes dades.
- La mateixa pantalla és accessible en qualsevol moment des de la UI.
- Un **servei injectat** proporciona aquestes dades a qualsevol operació de negoci
  (BL) que les necessiti.

## 2. Decisions de disseny

| Decisió | Què es fa | Per què |
|---|---|---|
| **On viu el servei** | Contracte a `BusinessLayer.Abstract/Generic/`, implementació a `BusinessLayer/Common/`, **Singleton** | És exactament el patró d'`INotificadorDeCanvis`: una peça transversal, ni entitat ni operació. Al `Generic` queda **fora** de l'escaneig de `BusinessLayer.DI.Injection.Contractes()`, que filtra pel namespace `…Abstract.Services` |
| **No és una `IBLOperation`** | El servei no hereta d'`IBLOperation` ni és `IDisposable` | Les operacions són d'un sol ús i Transient (`using var bl = …`). Aquest servei és de llarga vida i el llegeixen tant la UI com el BL |
| **Com hi arriben els ViewModels** | Nova propietat `DadesUsuari` a `IServiceFactory`, al costat de `Canvis` | El generic de `GetBLOperation<T>()` està acotat a `IBLOperation` i no el pot tornar. Injectar-lo pel constructor del ViewModel **trencaria** `FabricaDeServeisTest.ElsViewModelsAmbArgumentsDeRuntimeReepLaFabricaPelConstructor` (veure §10). El precedent de `Canvis` ja va obrir aquesta porta pel mateix motiu |
| **Com hi arriben les operacions de BL** | Pel constructor, sense fer res més | Les operacions es construeixen amb `ActivatorUtilities.CreateInstance(sp, …)` i el Singleton es registra abans. Una operació nova només ha de demanar `IDadesDeLusuari` al constructor |
| **Ubicació del fitxer** | La mateixa carpeta que `BaseDeDades.db` | La demana l'enunciat, i s'obté sola: `Data/` al costat de l'executable en RELEASE, `~/Documents/EapRecullData/` en DEBUG |
| **Format** | INI escrit i llegit amb un helper propi (~70 línies) | No hi ha cap paquet d'INI a la solució i no val la pena afegir-ne un. El format és de tres claus |
| **Lectura mandrosa** | El constructor **no toca el disc**; es llegeix al primer accés | `RegistreDITest` i `InjeccioTest` construeixen el contenidor sencer; cap `*ConfigureServices` ni cap resolució pot tocar el sistema de fitxers (invariant documentat a §7 d'`ARQUITECTURA.md`) |
| **Tolerància a fitxer corrupte** | Qualsevol error de lectura → `EstaInformat == false` + `Log.Error` | Un `.ini` amb una línia rara no pot impedir arrencar; el pitjor cas és que torni a demanar les dades |

## 3. Peces noves i peces tocades

### Fitxers nous

| Fitxer | Què hi ha |
|---|---|
| `BusinessLayer.Abstract/Generic/DadesUsuari.cs` | El registre `DadesUsuari(string Nom, string Cognoms, string AdrecaXtec)`, amb `Etiqueta`/`Descripcio` |
| `BusinessLayer.Abstract/Generic/IDadesDeLusuari.cs` | El contracte del servei |
| `BusinessLayer/Common/FitxerIni.cs` | Llegir/escriure INI preservant el que no coneixem |
| `BusinessLayer/Common/DadesDeLusuari.cs` | La implementació: càrrega mandrosa, validació, desat |
| `UI.ER.ViewModels/ViewModels/DadesUsuariViewModel.cs` | Tres propietats, `SubmitCommand`, `BrokenRules` |
| `UI.ER.AvaloniaUI/Pages/DadesUsuariWindow.axaml` (+ `.axaml.cs`) | El formulari |
| `BusinessLayer.Integration.Test/DadesUsuariTest.cs` | Tests del servei sobre carpeta temporal |

### Fitxers tocats

| Fitxer | Canvi |
|---|---|
| `DataLayer/AppOptionsBuilderConf.cs` | Extreure `CarpetaDeDades` de dins de `dataSource` i fer-la pública |
| `BusinessLayer/DI/Injection.cs` | `services.AddSingleton<IDadesDeLusuari, DadesDeLusuari>()`, al costat del bus |
| `BusinessLayer.Abstract/Generic/IServiceFactory.cs` | Propietat `IDadesDeLusuari DadesUsuari { get; }` |
| `UI.ER.ViewModels/Services/ServiceFactory.cs` | Implementar-la (una línia, com `Canvis`) |
| `UI.ER.ViewModels/ViewModels/AppStatusViewModel.cs` | `DadesUsuariCommand` + `ShowDadesUsuariDialog` + el porticó d'arrencada |
| `UI.ER.AvaloniaUI/Views/MainWindow.axaml` | Entrada de menú «Les meves dades» |
| `UI.ER.AvaloniaUI/Views/MainWindow.axaml.cs` | Una línia a `Registra(d)` |
| `UI.ER.AvaloniaUI.Test/RegistreDITest.cs` | Afegir `DadesUsuariViewModel` a la llista `necessaris` |

## 4. El contracte

```csharp
// BusinessLayer.Abstract/Generic/DadesUsuari.cs
namespace BusinessLayer.Abstract.Generic;

/// <summary>
/// Qui fa servir el programa. Implementa IEtiquetaDescripcio perquè encaixi a
/// OperationResult<T>, que és com la resta del BusinessLayer torna errors.
/// </summary>
public sealed record DadesUsuari(string Nom, string Cognoms, string AdrecaXtec)
    : IEtiquetaDescripcio
{
    public static DadesUsuari Buides { get; } = new(string.Empty, string.Empty, string.Empty);

    public string Etiqueta => $"{Nom} {Cognoms}".Trim();
    public string Descripcio => AdrecaXtec;
}
```

```csharp
// BusinessLayer.Abstract/Generic/IDadesDeLusuari.cs
namespace BusinessLayer.Abstract.Generic;

/// <summary>
/// Les dades de l'usuari, llegides d'un .ini al costat de la base de dades.
/// Singleton: un sol fitxer per a tota l'aplicació.
/// </summary>
/// <remarks>
/// No és una IBLOperation: no és d'un sol ús, no toca la base de dades i no
/// publica al bus de canvis. Segueix el patró d'INotificadorDeCanvis.
/// </remarks>
public interface IDadesDeLusuari
{
    /// <summary>Mai null: si el fitxer no hi és, són <see cref="DadesUsuari.Buides"/>.</summary>
    DadesUsuari Actuals { get; }

    /// <summary>Fals si el fitxer no existeix, no es pot llegir o li falta algun camp.</summary>
    bool EstaInformat { get; }

    /// <summary>El camí del fitxer, per poder-lo dir a l'usuari.</summary>
    string Ubicacio { get; }

    /// <summary>
    /// Valida i desa. Si hi ha BrokenRules no toca el disc i <see cref="Actuals"/>
    /// no canvia.
    /// </summary>
    Task<OperationResult<DadesUsuari>> Desa(DadesUsuari dades);
}
```

`Task<OperationResult<…>>` i no un retorn sec: és la forma que té tota la resta del
BusinessLayer i la que els ViewModels ja saben pintar (`BrokenRules2ModelView`).

## 5. El fitxer

```ini
; Dades de l'usuari d'EAP Recull.
; Aquest fitxer el manté el programa; es pot editar a mà amb cura.

[Usuari]
Nom=Dani
Cognoms=Herrera
AdrecaXtec=dh@xtec.cat
```

- **Nom**: `Usuari.ini` (§12, Q3), a `AppOptionsBuilderConf.CarpetaDeDades`.
- **Codificació**: UTF-8. Els cognoms catalans porten accents i ela geminada.
- **Lectura**: es descarten línies buides i les que comencen per `;` o `#`; `[Secció]`
  obre secció; la resta es parteix pel **primer** `=`. Claus i seccions es comparen
  sense distingir majúscules.
- **Escriptura**: es torna a escriure el fitxer sencer a partir del que s'ha llegit,
  de manera que **claus i seccions desconegudes es conserven**. Escriptura atòmica
  (fitxer temporal + `File.Move` amb sobreescriptura) per no deixar-lo a mitges.
- **Si el fitxer no existeix**: no es crea fins que l'usuari desa. Un fitxer buit no
  aporta res i confondria amb un d'informat.

## 6. Registre i cicle de vida

```csharp
// BusinessLayer/DI/Injection.cs, al costat del bus
services.AddSingleton<INotificadorDeCanvis, NotificadorDeCanvis>();
services.AddSingleton<IDadesDeLusuari, DadesDeLusuari>();
```

Afegit a la taula de §7 d'`ARQUITECTURA.md`:

| Servei | Vida | Per què |
|---|---|---|
| `IDadesDeLusuari` | **Singleton** | Un sol fitxer, llegit un cop; el desat n'actualitza la còpia en memòria i tothom la veu |

Com que es registra a `BusinessLayerConfigureServices()`, les dues eines de línia
d'ordres (`ImportData`, `CreateDemoData`) també el tenen sense tocar-les. En elles el
fitxer normalment no estarà informat: veure §12, Q4.

## 7. El camí de l'usuari

### Entrada de menú

`MainWindow.axaml`, dins del menú `DotsVertical`, abans de «Utilitats»:

```xml
<MenuItem Header="Les meves dades" Command="{Binding DadesUsuariCommand}">
    <MenuItem.Icon>
        <avalonia:MaterialIcon Kind="AccountEdit" Width="24" Height="24" />
    </MenuItem.Icon>
</MenuItem>
```

`AppStatusViewModel` guanya la parella que exigeix `NavegacioTest`:

```csharp
public ICommand DadesUsuariCommand { get; }
public Interaction<Unit, Unit> ShowDadesUsuariDialog { get; } = new();
```

i `MainWindow.Registra(d)` una línia:

```csharp
this.RegistraNavegacio<DadesUsuariWindow>(_windows, vm.ShowDadesUsuariDialog).DisposeWith(d);
```

`RegistraNavegacio<TWindow>(…, Interaction<Unit, Unit>)` ja existeix: és la
sobrecàrrega que fa servir `UtilitatsWindow`. **Res a registrar al contenidor**: la
convenció `DadesUsuariWindow` → `DadesUsuariViewModel` la resol sola la `WindowFactory`,
i l'escaneig de `UIConfigureServices` agafa les dues peces.

### Porticó d'arrencada

A `AppStatusViewModel`, dins de `WhenActivated`:

```csharp
this.WhenActivated(d =>
{
    // …la subscripció al bus que ja hi ha…

    // El porticó, un sol cop per sessió. Diferit amb el scheduler perquè els
    // RegisterHandler de la vista es registren a la mateixa passada d'activació:
    // llançar la Interaction aquí mateix pot arribar abans que el handler i petar
    // amb UnhandledUserInteractionException.
    if (!_jaComprovat && !_serveis.DadesUsuari.EstaInformat)
    {
        _jaComprovat = true;
        RxApp.MainThreadScheduler.Schedule(() =>
            ShowDadesUsuariDialog.Handle(Unit.Default).Subscribe());
    }
});
```

**Per què aquí i no a `App.OnFrameworkInitializationCompleted()`**: el cicle de vida
d'escriptori d'Avalonia vol una `MainWindow` assignada abans que res, i un
`ShowDialog` necessita propietari. A més, R4 va moure tota la navegació al ViewModel i
`NavegacioTest.MainWindowNoNavegaAmbHandlersDeClick` ho vigila. Aquest camí reaprofita
la canonada que ja hi és.

> El diferiment amb `RxApp.MainThreadScheduler.Schedule` és el mateix truc que ja fa
> servir `LoadData()` al constructor del mateix ViewModel. És el punt més delicat de la
> implementació i el primer a mirar si el diàleg no surt.

### La finestra

`DadesUsuariWindow.axaml`, modelada sobre `UtilitatsWindow` (`material:Card`, capçalera
amb icona, `Classes` de Material, cap color literal —ho vigila `DissenyTest`—, i
`x:DataType` + `x:CompileBindings="True"` —ho vigila `BindingsCompilatsTest`—):

- Tres `TextBox` amb `UseFloatingWatermark`: Nom, Cognoms, Adreça xtec.
- Un `ItemsControl` amb les `BrokenRules`, com fan les finestres de creació.
- Un text petit amb `Ubicacio`, perquè l'usuari sàpiga on és el fitxer.
- Botó «Desa». El code-behind només tanca la finestra quan el desat ha anat bé.

`DadesUsuariWindow.axaml.cs` necessita el constructor buit públic que exigeix
`ConstructorsDeVistaTest`; com que no demana la `IWindowFactory`, no cal el constructor
pont amb `App.Services`.

## 8. Com les fa servir una operació de negoci

Sense cap cerimònia:

```csharp
public class AlgunInforme : BLReport<Dtoo.SaveResult>, IAlgunInforme
{
    private readonly IDadesDeLusuari _usuari;

    public AlgunInforme(IDbContextFactory<AppDbContext> factory, IDadesDeLusuari usuari)
        : base(factory)
        => _usuari = usuari;

    // … _usuari.Actuals.Etiqueta a la plantilla …
}
```

**Cap operació no ha de cachejar `Actuals`**: és una propietat del Singleton i s'ha de
llegir a cada execució, perquè l'usuari pot haver editat les dades entremig.

Una operació que *exigeixi* les dades hauria d'afegir una regla al seu `RuleChecker`
(«Cal informar les dades de l'usuari abans de generar aquest informe») en comptes de
generar un document amb el nom en blanc.

## 9. Validació

Viu al servei, no al ViewModel: és una regla de negoci i és el que fa que valgui igual
per a la UI i per a qualsevol altre consumidor.

| Regla | Missatge |
|---|---|
| `Nom` no buit | «Cal informar el nom.» |
| `Cognoms` no buit | «Cal informar els cognoms.» |
| `AdrecaXtec` no buida | «Cal informar l'adreça xtec.» |
| `AdrecaXtec` amb forma d'adreça | «L'adreça xtec no té un format vàlid.» |

`EstaInformat` és, exactament, «les dades llegides passen aquestes regles». Així no hi
ha dues definicions de «informat» que puguin divergir.

El rigor de la quarta regla depèn de §12, Q2.

## 10. Tests

### Nous, a `BusinessLayer.Integration.Test/DadesUsuariTest.cs`

Construint `DadesDeLusuari` directament amb una carpeta temporal, sense contenidor:

- Fitxer inexistent → `EstaInformat == false`, `Actuals == DadesUsuari.Buides`, i **no
  s'ha creat cap fitxer**.
- `Desa` vàlid → `EstaInformat == true`, i una instància nova sobre la mateixa carpeta
  llegeix el mateix (anada i tornada).
- Accents i ela geminada sobreviuen l'anada i tornada.
- `Desa` amb un camp buit → `BrokenRules` no buides, `Actuals` **no** ha canviat i el
  fitxer del disc tampoc.
- Un `.ini` amb una secció i unes claus alienes → es conserven després de `Desa`.
- Un `.ini` escombraria → `EstaInformat == false` sense excepció.

### Que ja existeixen i s'han de mantenir verds

| Test | Què demana |
|---|---|
| `NavegacioTest.TotaInteraccioDelTaulellTeLaSevaComanda` | `ShowDadesUsuariDialog` obliga a `DadesUsuariCommand`. La convenció de noms del pla ja hi encaixa |
| `ConstructorsDeVistaTest` | `DadesUsuariWindow` amb constructor públic sense paràmetres |
| `RegistreDITest.EsRegistrenExactamentElsViewModels…` | `DadesUsuariViewModel(IServiceFactory)` entra sol a l'escaneig |
| `RegistreDITest.ElsViewModelsQueLaFactoryHaDeResoldreHiSon` | Cal **afegir-hi** `DadesUsuariViewModel` a mà: la llista és explícita |
| `FabricaDeServeisTest.ElsViewModelsAmbArgumentsDeRuntime…` | **Aquest és el que decideix §2**: la seva col·lecció de serveis és `UIConfigureServices()` tota sola, sense BusinessLayer. Un `DadesUsuariViewModel(IDadesDeLusuari)` hi sortiria com a «ViewModel amb arguments de runtime» i el test exigiria que el primer paràmetre fos `IServiceFactory`. Passant pel `IServiceFactory` el problema no existeix |
| `BindingsCompilatsTest`, `DissenyTest` | `x:CompileBindings="True"` i cap color literal al `.axaml` nou |
| `InjeccioTest` (BL) | Escaneja el namespace `…Abstract.Services`; `IDadesDeLusuari` és a `Generic` i en queda fora, com `INotificadorDeCanvis` |

## 11. Documentació a actualitzar

- `ARQUITECTURA.md`: taula de cicles de vida (§7), taula «Com afegir coses» (§12) i una
  línia a §6 sobre què més hi ha a la carpeta de dades.
- `agents.md`: el patró «servei transversal Singleton», al costat del bus.
- `UI.ER.AvaloniaUI/readme.md`: la finestra nova i el porticó d'arrencada.
- `docs/release_notes.md`.

## 12. Preguntes obertes

| # | Pregunta | Proposta per defecte |
|---|---|---|
| Q1 | Amb les dades sense informar, el diàleg d'arrencada **es pot tancar** sense omplir-lo? | Sí: es pot tancar i el programa funciona igual; el diàleg torna a sortir la propera arrencada. Bloquejar-lo deixaria l'usuari sense sortida si el disc és de només lectura |
| Q2 | «Adreça xtec» és un **correu** (`nom@xtec.cat`) i s'ha d'exigir el domini? | Validar només que tingui forma de correu, sense exigir domini. Hi ha `@edu.gencat.cat` i similars |
| Q3 | Nom del fitxer | `Usuari.ini` |
| Q4 | Què fan `ImportData` / `CreateDemoData` si el fitxer no hi és? | Res: continuen. Cap de les dues necessita avui les dades de l'usuari |
| Q5 | Hi ha ja algun lloc concret que **hagi de fer servir** aquestes dades (l'informe `AlumneInforme.cs.docx`, l'exportació pivot, un peu de pàgina)? | Aquest pla només les desa i les serveix. Si n'hi ha, ho afegim com a pas 7 |
| Q6 | Cal notificar la UI quan les dades canvien (per exemple, mostrar el nom a la barra superior)? | No de moment. Si calgués, un `event Action? Canviades` al servei, no el bus de domini |

## 13. Ordre d'implementació

1. `DataLayer`: extreure `CarpetaDeDades`. Compila i no canvia res de comportament.
2. `BusinessLayer.Abstract`: `DadesUsuari` + `IDadesDeLusuari`.
3. `BusinessLayer`: `FitxerIni` + `DadesDeLusuari` + registre a `Injection`.
4. Tests del servei. **A partir d'aquí el nucli és verd i verificat.**
5. `IServiceFactory` + `ServiceFactory`: la propietat nova.
6. `DadesUsuariViewModel` + `DadesUsuariWindow`, entrada de menú i `RegistraNavegacio`.
7. El porticó d'arrencada a `AppStatusViewModel`.
8. Ajustar `RegistreDITest`, passar la bateria sencera, documentació.

## 14. El que aquest pla no fa

- No desa **cap altra preferència** (tema, carpeta d'exportació, mides de finestra). Si
  més endavant n'hi ha, el `FitxerIni` ja hi és i només cal una secció nova.
- No posa les dades de l'usuari **a cap informe ni exportació** (Q5).
- No toca l'esquema de la base de dades: **cap migració**.
- No canvia on viu la base de dades.

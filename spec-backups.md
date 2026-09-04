# Pla: còpies de seguretat de la base de dades

> Estat: **proposta**, pendent de confirmar les decisions de §15.
> Context d'arquitectura: `ARQUITECTURA.md` §4 (operacions), §6 (persistència), §7 (DI),
> §8 (presentació). Precedent directe: `pla-fitxer-ini.md`, que va introduir el servei
> transversal `IDadesDeLusuari` i el `FitxerIni` que aquí es reaprofiten.

## 1. Què cal fer

Avui **no hi ha cap còpia de seguretat**. L'única cosa que hi ha és el fitxer buit amb nom
recordatori que `AppOptionsBuilderConf.CarpetaDeDades` deixa a la carpeta de dades la
primera vegada (`@@ Recorda Fer Copies Periodiques Del Fitxer BaseDeDades @@`). La base de
dades conté dades personals de menors —alumnes, informes NESE/NEE, actuacions— i viu en un
sol disc.

El que s'ha de poder fer, **a petició de l'usuari** (cap automatisme, cap programació):

1. L'usuari tria una **contrasenya**.
2. Es fa un bolcat consistent de la base de dades i s'empaqueta en un **zip xifrat AES-256**.
3. El zip va **al destí configurat**.
4. Se'n **guarden 3**; en arribar-ne una de nova, la més antiga sobrera desapareix.

El pas 3 és l'únic que canvia segons el destí, i per això és **l'única part que va darrere
d'un port** (`IMagatzemDeCopies`). Es preveuen dos adaptadors:

| Adaptador | Estat | Què fa |
|---|---|---|
| **Carpeta** (§7) | **El camí principal** | Desa el zip a una carpeta que tria l'usuari: un llapis USB, una unitat de xarxa o —i aquí és on puja al núvol— **la carpeta local d'un client de sincronització ja instal·lat i ja autoritzat** (Google Drive per a ordinadors, OneDrive, Nextcloud…) |
| **Google Drive per API** (§8) | **Condicionat a §2** | Obre el navegador, l'usuari autoritza un compte `@xtec.cat` i el zip puja per l'API de Drive |

L'ordre no és casual: **el segon depèn d'una cosa que no controlem** i el primer, no.

---

## 2. El punt crític: `@xtec.cat` probablement bloquejarà l'API

`xtec.cat` és un Google Workspace gestionat pel Departament d'Educació. A l'Admin console hi
ha un interruptor —*Security → Access and data control → API controls → App access control*—
que decideix què poden fer les aplicacions de tercers no configurades: **permetre-les o
bloquejar-les**. Als dominis d'educació és molt habitual que estigui a **bloquejar**, i per
als comptes de menors d'edat Google ho posa així **per defecte**.

Si està bloquejat, no hi ha res a fer des del codi: la pantalla de consentiment ni tan sols
apareix. Surt un *«Accés bloquejat: l'administrador no ha donat accés a aquesta
aplicació»* i s'acaba la conversa. Per això aquest pla **no hi juga la casa**.

### 2.1. Com comprovar-ho, de menys a més esforç

**A. Prova de 30 segons (indici, no prova).** Amb el compte `@xtec.cat` obert, entra a
<https://myaccount.google.com/connections>. Si hi surt **alguna aplicació de tercers** que no
sigui de Google, vol dir que el domini no ho bloqueja tot.

> **Comprovat el 2026-09-04**: hi surten ClassQuiz, Kahoot i companyia. El domini **no**
> bloqueja les aplicacions de tercers en bloc.

Ara bé, l'indici té una lletra petita que cal llegir. A l'Admin console, el bloqueig
d'aplicacions no configurades porta una casella a part que permet **iniciar sessió** amb
aplicacions que no demanen accés a cap servei de Google. Kahoot i ClassQuiz són exactament
això: «Inicia sessió amb Google», que només demana nom, adreça i foto. Un domini pot tenir
la sessió oberta i **l'accés a dades tancat**, i llavors la llista de connexions es veu
igual de plena però el Drive segueix barrat.

**El refinament, i són 30 segons més**: a la mateixa pàgina, clica una d'aquestes
aplicacions i mira què diu a «Té accés a».

| El que hi diu | Què vol dir |
|---|---|
| Només «Informació personal», «adreça electrònica», «nom i foto» | Sessió i prou. **Encara no sabem res del Drive**: cal la prova B |
| Alguna que sí que llista un servei amb dades —Drive, Gmail, Calendar, Classroom | **Concloent i bo.** El domini deixa passar abasts de dades, i el nostre (`drive.file`) és el més petit de tots |

Si la llista és buida o només hi ha coses de Google, és mal senyal —però tampoc concloent:
pot ser simplement que no n'hagis connectat mai cap.

**B. Prova definitiva, ~15 minuts i sense escriure ni una línia de codi.**

1. A <https://console.cloud.google.com>, **amb un compte personal de Gmail** (no cal l'xtec,
   i probablement l'xtec tampoc et deixarà crear projectes): projecte nou → *APIs & Services*
   → *OAuth consent screen* → tipus **External** → **Publish app** («In production») →
   *Credentials* → *Create credentials* → *OAuth client ID* → **Desktop app**. Copia'n el
   `client_id`.
2. Al navegador, **amb la sessió de `@xtec.cat` iniciada**, obre aquesta adreça posant-hi el
   teu `client_id` (tot en una sola línia):

   ```
   https://accounts.google.com/o/oauth2/v2/auth?client_id=EL_TEU_CLIENT_ID&redirect_uri=http://127.0.0.1:8080&response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fdrive.file&access_type=offline
   ```

3. Llegeix què surt:

   | El que veus | Què vol dir |
   |---|---|
   | La pantalla de consentiment («… vol accedir al teu compte de Google», potser amb l'avís d'aplicació no verificada) | **No està bloquejat.** L'adaptador de Drive és viable. Accepta i deixa que el navegador falli connectant a `127.0.0.1:8080`: això ja és el final feliç de la prova |
   | «Accés bloquejat», «l'administrador del teu domini…», `admin_policy_enforced` | **Està bloquejat.** Cal l'allowlist de §2.2 o anar per la carpeta (§7) |
   | «Aquesta aplicació no està verificada» amb un enllaç *Avançat → Continua* | Només és l'avís de verificació, **no** un bloqueig. Es resol publicant o demanant l'allowlist |

   La prova és fidel perquè fa servir **exactament** el mateix flux, el mateix abast i el
   mateix tipus de client que faria servir el programa.

**C. Preguntar-ho directament.** Un correu a qui administri el domini estalvia B, i de
totes maneres caldrà parlar-hi si es vol l'allowlist.

### 2.2. Si està bloquejat: què s'ha de demanar a l'administrador

És una petició petita i concreta, i val la pena fer-la encara que anem per la carpeta:

> A l'Admin console → *Security* → *Access and data control* → *API controls* → *Manage
> Third-Party App Access* → **Configure new app** → *OAuth App Name Or Client ID* → hi
> enganxes el `client_id` d'EAP Recull i el marques com a **Trusted**, limitat a l'abast
> `https://www.googleapis.com/auth/drive.file`.

Arguments que la fan fàcil d'acceptar:

- `drive.file` és **l'abast més petit que existeix** per a Drive: l'aplicació **només veu
  els fitxers que ella mateixa crea**. No pot llegir, ni llistar, ni tocar cap altre
  document del Drive de ningú. Per a Google és un abast *no sensible* i no requereix la
  verificació de seguretat que sí que demanen `drive` o `drive.readonly`.
- El fitxer que puja va **xifrat des de l'ordinador de l'usuari**: al Drive hi arriba text
  xifrat que ni Google ni l'administrador poden obrir.
- El codi és lliure i auditable, i l'allowlist és per `client_id`, revocable en qualsevol
  moment.

**Efecte lateral útil**: un client marcat com a *Trusted* també deixa d'ensenyar l'avís
d'«aplicació no verificada» als usuaris.

### 2.3. I si no hi ha manera

Es fa el pla sense l'adaptador de Drive. **No es perd gairebé res**: la carpeta de §7 puja
igualment al núvol si l'usuari hi tria la carpeta local del client de sincronització de
Drive o de OneDrive, que ja està instal·lat i **ja té el vistiplau de l'administrador**. La
diferència pràctica per a l'usuari és que en comptes d'autoritzar un compte, tria una
carpeta un sol cop.

### 2.4. Altres destins valorats

| Alternativa | Veredicte |
|---|---|
| **Carpeta local o de xarxa** (llapis USB, unitat compartida) | **Sí.** És l'adaptador de §7. Zero dependències, zero credencials, i cobreix el cas de qui no vol núvol |
| **Carpeta d'un client de sincronització ja instal·lat** (Drive per a ordinadors, OneDrive, Nextcloud) | **Sí, i és la resposta a la pregunta.** Puja al núvol **sense API, sense OAuth i sense permís de ningú**, perquè qui parla amb el núvol és un programa que el domini ja autoritza. Mateix adaptador de §7: per a EAP Recull és una carpeta i prou |
| **API de Drive amb compte `@xtec.cat`** | Condicionat a §2 |
| **API de Drive amb compte personal de Gmail** | Tècnicament funciona sempre (cap administrador pel mig), però posa dades d'alumnes —encara que xifrades— en un compte particular. Institucionalment és pitjor que la carpeta. Només com a últim recurs, i amb la regla de domini relaxada (§15, Q2) |
| **OneDrive / Microsoft 365 per API** | Mateix problema exacte: l'Entra ID del domini també demana consentiment d'administrador per a aplicacions de tercers. Canviar de núvol no canvia la política |
| **Nextcloud o WebDAV** | Viable i senzill (usuari + contrasenya, sense OAuth) **si el Departament n'ofereix un**. Seria un tercer adaptador de 60 línies sobre el mateix port |
| **Enviar-se el zip per correu** | Descartat. Gmail talla als 25 MB i els Workspace acostumen a tenir l'SMTP amb contrasenya desactivat |

---

## 3. Decisions de disseny

| Decisió | Què es fa | Per què |
|---|---|---|
| **El destí és un port** | `IMagatzemDeCopies` a `BusinessLayer.Abstract/Generic/`, amb `MagatzemDeCarpeta` i (si escau) `MagatzemDrive` a `BusinessLayer/Common/` | És el que fa que §2 no sigui un blocador: tot el que és car —bolcat, verificació, xifratge, retenció, UI, tests— és **comú**, i el destí és la peça petita i intercanviable. També és el que permet provar-ho tot **sense xarxa**, amb un magatzem fals |
| **La carpeta primer** | L'adaptador de carpeta es fa i es publica abans que el de Drive | No depèn de ningú, cobreix el cas real (USB, unitat de xarxa i carpeta sincronitzada) i deixa el programa amb còpies de seguretat **aquesta setmana** en comptes de d'aquí a tres correus amb l'administrador |
| **No cal tancar cap connexió** | La còpia es fa amb `VACUUM INTO`, no copiant bytes del fitxer | És l'API de SQLite pensada exactament per això: una còpia **consistent** d'una base de dades viva, des d'una connexió qualsevol, sense bloquejar els altres. Copiar `BaseDeDades.db` amb `File.Copy` mentre hi ha una transacció oberta dona un fitxer corrupte, i tancar connexions «a mà» no es pot fer: la `IDbContextFactory` n'obre i en tanca a cada operació i `Microsoft.Data.Sqlite` en manté un *pool*. Efecte lateral: la còpia surt **compactada** i sense `-wal`/`-journal` |
| **On viu l'operació** | `ICopiaDeSeguretat` a `BusinessLayer.Abstract/Services/` + `CopiaDeSeguretat` a `BusinessLayer/Services/` | Operació de negoci d'un sol ús: entra pel registre per convenció (`IXxx` → `Xxx`) sense tocar `Injection.cs`, i el ViewModel la consumeix amb `using var bl = …` com totes les altres |
| **El ViewModel no veu el magatzem** | Tot passa per `ICopiaDeSeguretat` | Evita una tercera propietat a `IServiceFactory`. El destí és infraestructura; la UI només ha de conèixer l'operació |
| **Zip xifrat AES-256** | `SharpZipLib`, `AESKeySize = 256` | `System.IO.Compression` **no sap xifrar**. L'alternativa clàssica (ZipCrypto, la que obre l'explorador de Windows sense res més) està trencada des dels anys 90, i aquí hi ha dades de menors. Contrapartida a documentar: cal 7-Zip, WinRAR, Keka o similar per obrir-lo |
| **La contrasenya no es desa** | Ni a l'`.ini`, ni al log, ni enlloc | És l'única cosa que separa el destí de les dades. Es demana a cada còpia, amb confirmació, i es buida del ViewModel en acabar |
| **Metadades a l'`Usuari.ini`** | Secció `[CopiaDeSeguretat]`: destí triat, carpeta o compte, i data de la darrera còpia | És literalment el que diu la taula «Com afegir coses» d'`ARQUITECTURA.md` §12 per a una preferència desada a disc, i `FitxerIni` ja conserva el que no coneix |
| **Primer desar, després esborrar** | La retenció s'aplica **només** després que la còpia nova hi sigui de debò | Si s'esborrés abans i la còpia fallés a mitges, l'usuari es quedaria amb dues còpies en comptes de tres i cap de nova |
| **Un fallada de retenció no fa fallar la còpia** | Es registra i s'avisa, però el resultat és correcte | La còpia nova ja és a lloc, que és el que importa |

---

## 4. Peces noves i peces tocades

### Fitxers nous

| Fitxer | Què hi ha | Fase |
|---|---|---|
| `BusinessLayer.Abstract/Services/ICopiaDeSeguretat.cs` | El contracte de l'operació | 1 |
| `BusinessLayer.Abstract/Generic/IMagatzemDeCopies.cs` | El port cap al destí | 1 |
| `BusinessLayer.Abstract/Generic/CopiaRemota.cs` | Els registres `CopiaRemota`, `Copies` i `DestiDeCopies` | 1 |
| `DTO.o/DTOs/CopiaResult.cs` | El resultat que veu la UI | 1 |
| `BusinessLayer/Common/ZipXifrat.cs` | Empaquetar amb contrasenya (AES-256) | 1 |
| `BusinessLayer/Common/MagatzemDeCarpeta.cs` | L'adaptador de carpeta | 1 |
| `BusinessLayer/Services/CopiaDeSeguretat.cs` | L'orquestració: bolcat → verificació → zip → desat → retenció | 1 |
| `UI.ER.ViewModels/ViewModels/CopiaDeSeguretatViewModel.cs` | Contrasenya, destí, estat, progrés, llista de còpies | 1 |
| `UI.ER.AvaloniaUI/Pages/CopiaDeSeguretatWindow.axaml` (+ `.axaml.cs`) | La finestra i el selector de carpeta | 1 |
| `BusinessLayer.Integration.Test/CopiaDeSeguretatTest.cs` | L'operació sencera contra un magatzem fals | 1 |
| `BusinessLayer.Integration.Test/MagatzemFals.cs` | El doble del port | 1 |
| `BusinessLayer/Common/MagatzemDrive.cs` | L'adaptador de Google Drive | 2 |

### Fitxers tocats

| Fitxer | Canvi | Fase |
|---|---|---|
| `BusinessLayer/BusinessLayer.csproj` | `SharpZipLib` (fase 1) i `Google.Apis.Drive.v3` (fase 2) | 1 i 2 |
| `BusinessLayer/DI/Injection.cs` | `AddSingleton<IMagatzemDeCopies, …>()` per cada adaptador | 1 i 2 |
| `UI.ER.ViewModels/ViewModels/AppStatusViewModel.cs` | `CopiaDeSeguretatCommand` + `ShowCopiaDeSeguretatDialog` | 1 |
| `UI.ER.AvaloniaUI/Views/MainWindow.axaml` (+ `.axaml.cs`) | Entrada de menú «Còpia de seguretat» + una línia a `Registra(d)` | 1 |
| `UI.ER.AvaloniaUI.Test/RegistreDITest.cs` | Afegir `CopiaDeSeguretatViewModel` a la llista `necessaris` | 1 |
| `README.md`, `ARQUITECTURA.md`, `agents.md`, `docs/release_notes.md` | §14 | 1 |

---

## 5. Els contractes

```csharp
// BusinessLayer.Abstract/Generic/CopiaRemota.cs
namespace BusinessLayer.Abstract.Generic;

/// <summary>Una còpia que ja és al destí.</summary>
public sealed record CopiaRemota(string Id, string Nom, DateTime Creada, long Bytes)
    : IEtiquetaDescripcio
{
    public string Etiqueta => Nom;
    public string Descripcio => $"{Creada:dd/MM/yyyy HH:mm} · {Bytes / 1024d / 1024d:N1} MB";
}

/// <summary>
/// Embolcall d'una llista de còpies: <see cref="OperationResult{T}"/> exigeix
/// <c>T : IEtiquetaDescripcio</c> i una llista pelada no ho compleix. Mateix cas que
/// <c>ImportAllResult</c>.
/// </summary>
public sealed record Copies(IReadOnlyList<CopiaRemota> Items) : IEtiquetaDescripcio
{
    public string Etiqueta => $"{Items.Count} còpies";
    public string Descripcio => string.Join(" · ", Items.Select(c => c.Nom));
}

/// <summary>
/// On van les còpies, tal com s'ha de poder ensenyar a l'usuari. Per a la carpeta,
/// «Carpeta» + el camí; per al Drive, el correu del compte + la carpeta remota.
/// </summary>
public sealed record DestiDeCopies(string Nom, string Detall) : IEtiquetaDescripcio
{
    public static DestiDeCopies Cap { get; } = new(string.Empty, "Cap destí configurat");

    public bool Configurat => !string.IsNullOrEmpty(Nom);
    public string Etiqueta => Nom;
    public string Descripcio => Detall;
}
```

```csharp
// BusinessLayer.Abstract/Generic/IMagatzemDeCopies.cs
namespace BusinessLayer.Abstract.Generic;

/// <summary>
/// On van a parar les còpies. La interfície no diu enlloc si és una carpeta o un núvol,
/// i és el que permet provar l'operació sense xarxa i afegir destins nous sense tocar-la.
/// </summary>
/// <remarks>
/// Singleton: la configuració —i, al Drive, la sessió autoritzada— és una de sola per a
/// tot el programa. El constructor <strong>no</strong> pot tocar ni el disc ni la xarxa,
/// igual que <see cref="IDadesDeLusuari"/>: <c>InjeccioTest</c> construeix el contenidor
/// sencer i resol totes les operacions, i una d'elles el demana pel constructor.
/// </remarks>
public interface IMagatzemDeCopies
{
    /// <summary>«carpeta», «drive»… El que es desa a l'<c>Usuari.ini</c> per triar destí.</summary>
    string Clau { get; }

    /// <summary>Com es diu, per pintar-ho al selector: «Carpeta», «Google Drive».</summary>
    string Titol { get; }

    /// <summary>On van les còpies ara mateix, sense obrir res ni connectar enlloc.</summary>
    DestiDeCopies Desti { get; }

    /// <summary>
    /// Deixa el magatzem llest: la carpeta comprova que existeix i s'hi pot escriure;
    /// el Drive obre el navegador si no té credencials vives. Idempotent.
    /// </summary>
    Task<OperationResult<DestiDeCopies>> Prepara(CancellationToken ct);

    /// <summary>Hi desa un fitxer local. Torna la còpia tal com ha quedat al destí.</summary>
    Task<OperationResult<CopiaRemota>> Desa(
        string camiLocal, string nomDesti, IProgress<long>? bytesEscrits, CancellationToken ct);

    /// <summary>Les còpies que hi ha, de la més nova a la més vella.</summary>
    Task<OperationResult<Copies>> Llista(CancellationToken ct);

    /// <summary>Deixa només les <paramref name="quantes"/> més noves. Torna les retirades.</summary>
    Task<OperationResult<Copies>> RetiraSobrants(int quantes, CancellationToken ct);

    /// <summary>Oblida la configuració (carpeta triada, credencials desades). Idempotent.</summary>
    Task<OperationResult<DestiDeCopies>> Oblida();
}
```

```csharp
// BusinessLayer.Abstract/Services/ICopiaDeSeguretat.cs
namespace BusinessLayer.Abstract.Services;

/// <summary>
/// Fa una còpia de seguretat de la base de dades i la porta al destí configurat.
/// Operació d'un sol ús: <c>using var bl = serveis.GetBLOperation&lt;ICopiaDeSeguretat&gt;()</c>.
/// </summary>
public interface ICopiaDeSeguretat : IBLOperation
{
    /// <summary>Els destins disponibles en aquesta compilació, per al selector de la finestra.</summary>
    IReadOnlyList<DestiDisponible> Destins { get; }

    /// <summary>Quin hi ha triat i com està configurat.</summary>
    DestiDeCopies DestiActual { get; }

    /// <param name="password">La que xifra el zip. No es desa enlloc.</param>
    /// <param name="progres">Missatges per a la UI: «Preparant la còpia…», «Desant…».</param>
    Task<OperationResult<Dtoo.CopiaResult>> Run(
        string password, IProgress<string>? progres = null, CancellationToken ct = default);

    /// <summary>Tria i prepara un destí (carpeta triada, o autorització del compte).</summary>
    Task<OperationResult<DestiDeCopies>> ConfiguraDesti(
        string clau, string? parametre, CancellationToken ct = default);

    /// <summary>Les còpies que hi ha al destí, per pintar-les a la finestra.</summary>
    Task<OperationResult<Copies>> Llista(CancellationToken ct = default);

    /// <summary>Oblida el destí configurat.</summary>
    Task<OperationResult<DestiDeCopies>> Oblida();
}
```

```csharp
// DTO.o/DTOs/CopiaResult.cs
public class CopiaResult : IDTOo, IEtiquetaDescripcio
{
    public CopiaResult(string nom, DateTime quan, long bytes, string desti,
                       int copiesGuardades, int actuacions, string? avis) { … }

    public string Etiqueta => $"Còpia «{Nom}» desada a {Desti}";
    public string Descripcio => $"{Bytes / 1024d / 1024d:N1} MB · {Actuacions:N0} actuacions · se'n guarden {CopiesGuardades}";
}
```

L'operació es construeix sola pel registre per convenció:

```csharp
public class CopiaDeSeguretat : BLOperation, ICopiaDeSeguretat
{
    private readonly IReadOnlyList<IMagatzemDeCopies> _magatzems;
    private readonly IDadesDeLusuari _usuari;

    public CopiaDeSeguretat(
        IDbContextFactory<AppDbContext> factory,
        IEnumerable<IMagatzemDeCopies> magatzems,   // un o dos, segons la fase
        IDadesDeLusuari usuari) : base(factory) { … }
}
```

`ActivatorUtilities.CreateInstance` resol els tres paràmetres perquè els magatzems es
registren a `BusinessLayerConfigureServices()` **abans** del bucle d'operacions. L'`IEnumerable`
és el que fa que afegir l'adaptador de Drive més endavant sigui **una línia al registre i cap
canvi a l'operació**.

---

## 6. El camí de la còpia, pas a pas

Comú a tots els destins, i tot dins d'un `try/catch` que acaba en `OperationResult` amb
`BrokenRules`: cap excepció crua cap amunt (invariant 5 d'`ARQUITECTURA.md`).

### 6.1. Carpeta de treball

Una carpeta temporal nova per execució (`Path.Combine(Path.GetTempPath(), "EapRecull-" + Guid)`),
esborrada en un `finally` **passi el que passi**: no s'hi pot quedar mai un `.db` en clar.

### 6.2. Bolcat consistent

```csharp
// El path va literal dins de l'SQL perquè VACUUM INTO no accepta paràmetres a tot arreu;
// s'escapen les cometes simples, que és l'únic metacaràcter d'un literal SQLite.
var desti = Path.Combine(temporal, "BaseDeDades.db");
var literal = desti.Replace("'", "''");
await GetContext().Database.ExecuteSqlRawAsync($"VACUUM INTO '{literal}'", ct);
```

- `VACUUM INTO` (SQLite ≥ 3.27; el `SQLitePCLRaw.bundle_e_sqlite3` 3.0.2 del projecte en porta
  de molt més moderna) escriu una base de dades **nova, consistent i compactada**.
- El destí **no pot existir**: per això la carpeta és nova a cada execució.
- Cap altra connexió no queda bloquejada més enllà del que dura el bolcat.

### 6.3. Verificació

Abans de xifrar res, s'obre la còpia **en només lectura** i es comprova:

```sql
PRAGMA integrity_check;      -- ha de tornar exactament "ok"
SELECT COUNT(*) FROM Actuacions;
```

El recompte va al `CopiaResult` i es pinta a la finestra: és el que permet a l'usuari veure
d'un cop d'ull que la còpia té les dades que espera. Si `integrity_check` no diu `ok`,
s'atura amb una `BrokenRule` i **no es desa res**.

### 6.4. El zip

| Entrada | Per què hi és |
|---|---|
| `BaseDeDades.db` | El bolcat |
| `Usuari.ini` | Si existeix. És minúscul i completa la restauració |
| `LLEGEIX-ME.txt` | Data, versió del programa i **les instruccions per restaurar** (§14) |

```csharp
using var sortida = new ZipOutputStream(File.Create(camiZip));
sortida.SetLevel(6);
sortida.Password = password;          // AES-256 gràcies a AESKeySize, no ZipCrypto

var entrada = new ZipEntry(nom) { DateTime = DateTime.Now, AESKeySize = 256 };
```

**Nom del fitxer**: `EAPRecull-BaseDeDades-2026-09-04_1215.zip`. Data i hora al nom perquè
ordenin sols i perquè l'usuari sàpiga què té sense obrir res.

### 6.5. El desat al destí

`Prepara(ct)` primer; si el destí no està llest —carpeta que ja no existeix, autorització
cancel·lada— s'acaba aquí amb una `BrokenRule` amable i **sense** haver deixat res a mig fer.
Després `Desa(...)`, amb l'`IProgress` connectat: ni una còpia a un llapis USB ni una pujada
per una línia d'escola són instantànies, i la finestra ha de dir alguna cosa.

### 6.6. La retenció

Només **després** d'un desat correcte:

```
llista el destí (de la més nova a la més vella)
si n'hi ha més de 3 → les de la posició 4 endavant es retiren
```

Un fallada aquí **no fa fallar la còpia**: es registra al log i s'omple el camp `Avis` del
resultat.

### 6.7. Estat final

S'actualitza `[CopiaDeSeguretat]` de l'`Usuari.ini` (destí, paràmetre, data) i es torna el
`CopiaResult`.

---

## 7. Adaptador 1: carpeta

El camí principal. Cinquanta línies mal comptades, cap dependència nova, i cobreix tres
casos d'ús amb el mateix codi:

| L'usuari tria… | I això vol dir… |
|---|---|
| `E:\CopiesEAP` (llapis USB) | Còpia fora de l'ordinador, sense núvol |
| `\\servidor\eap\copies` (unitat de xarxa) | Còpia al servidor del centre |
| `~/Google Drive/Còpies EAP` o `~/OneDrive/…` | **Còpia al núvol**, pujada pel client de sincronització que el domini ja té instal·lat i autoritzat |

L'última fila és la resposta a «hi ha cap altra manera de pujar el fitxer»: n'hi ha, i no
demana permís a ningú, perquè EAP Recull no parla amb Google —hi parla el client de Drive,
que ja hi parlava abans.

Implementació:

- `Prepara`: la carpeta existeix i s'hi pot escriure (prova real: crea i esborra un fitxer
  temporal). Si no, `BrokenRule` amb el camí, que és el que l'usuari necessita per adonar-se
  que el llapis no està endollat.
- `Desa`: `File.Copy` del zip cap a la carpeta. **Escriptura atòmica**: es copia a
  `<nom>.tmp` i es fa `File.Move` al nom bo, perquè un client de sincronització no comenci a
  pujar un fitxer a mig escriure.
- `Llista`: `Directory.EnumerateFiles(carpeta, "EAPRecull-BaseDeDades-*.zip")`, ordenat per
  data d'escriptura descendent. **El patró del nom és el que delimita què és nostre**: mai
  no es toca cap altre fitxer de la carpeta que l'usuari hagi triat.
- `RetiraSobrants`: `File.Delete` de les sobreres. Aquí sí que és definitiu —a diferència del
  Drive, no hi ha paperera fiable a totes les plataformes—, i per això la finestra avisa
  quantes se'n guarden.
- `Oblida`: buida la clau de l'`.ini`. **No esborra cap fitxer.**

**Avís que ha de donar la UI**: si la carpeta triada és al **mateix volum** que la carpeta de
dades, això no és una còpia de seguretat de debò —un disc que mor se les emporta totes dues—.
No es prohibeix (pot ser un pas intermedi legítim), però es diu clarament.

---

## 8. Adaptador 2: Google Drive per API *(condicionat a §2)*

### 8.1. Client OAuth

- Tipus **«Aplicació d'escriptori»** a Google Cloud Console: és el que permet el flux de
  *loopback* (`LocalServerCodeReceiver` obre un port a `127.0.0.1`, hi rep el codi i tanca).
- Abasts: `https://www.googleapis.com/auth/drive.file` i `email`.
- **L'estat de publicació ha de ser «En producció»**, no «Proves». En mode proves Google
  caduca els *refresh tokens* **als 7 dies** i l'usuari hauria de tornar a autoritzar cada
  setmana: és un error caríssim de diagnosticar més tard.

### 8.2. El secret del client

L'`client_id`/`client_secret` d'una aplicació instal·lada **no és un secret de debò** —Google
ho documenta així—, però tampoc no ha d'anar al repositori públic en clar:

1. Els valors arriben per un `Google.ini` a la carpeta de dades **o** per variables d'entorn,
   i si no hi són es fa servir el que s'hagi incrustat en compilar.
2. Al *release* els incrusta el workflow a partir de *secrets* de GitHub Actions
   (`GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`).
3. **Sense credencials configurades, el destí «Google Drive» no surt al selector.** Qui es
   compili la seva versió —el README ho convida a fer-ho— hi ha de poder posar el seu client
   sense tocar codi.

### 8.3. El compte ha de ser `@xtec.cat`

```csharp
var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
{
    ClientSecrets = secrets,
    Scopes = new[] { DriveService.Scope.DriveFile, "email" },
    DataStore = new FileDataStore(carpetaCredencials, fullPath: true),
});

// L'adreça de l'Usuari.ini com a login_hint: el navegador ja proposa el compte bo.
var credencial = await new AuthorizationCodeInstalledApp(flow, new LocalServerCodeReceiver())
    .AuthorizeAsync(usuariId: "eaprecull", ct);
```

- El `login_hint` **no és cap garantia**: l'usuari pot triar-ne un altre. La comprovació de
  debò es fa **després**, amb el correu verificat de l'`id_token`
  (`GoogleJsonWebSignature.ValidateAsync`), no amb el que hagi escrit ningú.
- Si el correu no acaba en `@xtec.cat`: s'esborren les credencials acabades de desar, es
  revoca el permís i es torna una `BrokenRule` que **diu quin compte s'ha triat** («El compte
  triat (algu@gmail.com) no és del domini xtec.cat»). Un missatge genèric aquí és una tarda
  perduda.
- És una **regla de negoci, no una frontera de seguretat**: viu a l'adaptador, es prova amb
  un test i es canvia en un sol lloc (§15, Q2).

### 8.4. On van les credencials

```
Windows  %APPDATA%\EapRecull\Credencials\
Linux    ~/.config/EapRecull/Credencials/
macOS    ~/Library/Application Support/EapRecull/Credencials/
```

(`Environment.SpecialFolder.ApplicationData` a les tres.) A Unix, `File.SetUnixFileMode` les
deixa a `600`. **No** van a `CarpetaDeDades`: el programa es distribueix com un zip i es pot
executar des d'un llapis USB, i el testimoni de refresc hi viatjaria. `Oblida()` esborra la
carpeta i crida `https://oauth2.googleapis.com/revoke`; que la revocació falli sense xarxa no
ha d'impedir esborrar el fitxer local.

### 8.5. Desat i retenció

- Carpeta remota `EAP Recull - Còpies de seguretat`, creada per l'aplicació; el seu id es desa
  a l'`Usuari.ini`. Si l'id ja no existeix, es torna a buscar entre les carpetes que ha creat
  l'aplicació i, si no hi és, se'n crea una de nova.
- Pujada **resumible** (`FilesResource.CreateMediaUpload` amb `ChunkSize`), amb
  `ProgressChanged` connectat a l'`IProgress`.
- Retenció **a la paperera** (`trashed = true`), no esborrat permanent: 30 dies de marge si
  algú s'adona que necessitava aquella còpia, i la paperera es buida sola.

### Paquets

| Paquet | Versió | Fase |
|---|---|---|
| `SharpZipLib` | 1.4.2 | 1 |
| `Google.Apis.Drive.v3` | 1.76.0.4252 (arrossega `Google.Apis.Auth`) | 2 |

Cap dels dos surt de `BusinessLayer`: la regla que diu que `UI.ER.ViewModels` només veu
`BusinessLayer.Abstract` es manté intacta, i els ViewModels no saben que Google existeix.

---

## 9. Registre i cicle de vida

```csharp
// BusinessLayer/DI/Injection.cs, al costat dels altres dos singletons
services.AddSingleton<INotificadorDeCanvis, NotificadorDeCanvis>();
services.AddSingleton<IDadesDeLusuari, DadesDeLusuari>();
services.AddSingleton<IMagatzemDeCopies, MagatzemDeCarpeta>();   // fase 1
// services.AddSingleton<IMagatzemDeCopies, MagatzemDrive>();    // fase 2
```

Afegit a la taula de §7 d'`ARQUITECTURA.md`:

| Servei | Vida | Per què |
|---|---|---|
| `IMagatzemDeCopies` | **Singleton** (un per adaptador) | La configuració del destí —i, al Drive, la sessió autoritzada— és una de sola. L'`ICopiaDeSeguretat`, que és Transient, els rep tots com a `IEnumerable` |

`ICopiaDeSeguretat` no cal registrar-la: hi entra per convenció, com les altres 30.

> **Invariant que cal respectar**: cap constructor d'adaptador llegeix l'`.ini`, crea
> carpetes, llegeix credencials ni obre cap `DriveService`. Tot és mandrós, dins de
> `Prepara`. `InjeccioTest.CadaContracteTeLaSevaImplementacioPerConvencio` resol **totes** les
> operacions del registre, i això construiria els magatzems en un CI sense xarxa ni navegador.

---

## 10. El camí de l'usuari

### Entrada de menú

`MainWindow.axaml`, al menú `DotsVertical`, entre «Les meves dades» i «Utilitats»:

```xml
<MenuItem Header="Còpia de seguretat" Command="{Binding CopiaDeSeguretatCommand}">
    <MenuItem.Icon>
        <avalonia:MaterialIcon Kind="DatabaseExport" Width="24" Height="24" />
    </MenuItem.Icon>
</MenuItem>
```

`AppStatusViewModel` guanya la parella que exigeix `NavegacioTest`:

```csharp
public ICommand CopiaDeSeguretatCommand { get; }
public Interaction<Unit, Unit> ShowCopiaDeSeguretatDialog { get; } = new();
```

i `MainWindow.Registra(d)` una línia, amb la sobrecàrrega d'`Interaction<Unit, Unit>` que ja
fan servir `UtilitatsWindow` i `DadesUsuariWindow`:

```csharp
this.RegistraNavegacio<CopiaDeSeguretatWindow>(_windows, vm.ShowCopiaDeSeguretatDialog).DisposeWith(d);
```

Res a registrar al contenidor: la convenció `CopiaDeSeguretatWindow` →
`CopiaDeSeguretatViewModel` la resol sola la `WindowFactory`.

### La finestra

Modelada sobre `UtilitatsWindow` (`material:Card`, capçalera amb icona, `Classes` de
Material, cap color literal —`DissenyTest`—, `x:DataType` + `x:CompileBindings="True"`
—`BindingsCompilatsTest`—). Tres blocs:

1. **El destí**
   - Selector amb els destins disponibles (a la fase 1 només «Carpeta»; amb un sol destí,
     ni cal ensenyar el selector).
   - Per a la carpeta: el camí actual i un botó «Tria la carpeta…».
   - Per al Drive: el compte connectat i els botons «Connecta»/«Desconnecta», amb la línia
     *«EAP Recull només podrà veure i gestionar els fitxers que ell mateix hagi creat al teu
     Drive»*.
   - El consell de posar-hi una carpeta sincronitzada o un llapis, i l'avís de «mateix disc»
     de §7.

2. **La còpia**
   - `TextBox` amb `PasswordChar` per a la contrasenya, i un altre per repetir-la.
   - Un avís ben visible: **si es perd la contrasenya, la còpia no es pot recuperar**.
   - Botó «Fes la còpia ara», `IndicadorCarrega` mentre dura i botó «Cancel·la» lligat al
     `CancellationTokenSource`.
   - Text d'estat alimentat per l'`IProgress<string>`: *Preparant la còpia… ·
     Comprovant-la… · Xifrant… · Desant (3,4 de 8,1 MB)… · Fet*.

3. **Les còpies que hi ha**
   - Un `ItemsControl` amb les `CopiaRemota` (`Etiqueta` i `Descripcio` ja venen fetes) i la
     data de la darrera còpia.
   - `ItemsControl` de `BrokenRules`, com la resta de finestres.

`CopiaDeSeguretatWindow.axaml.cs` necessita el constructor públic sense paràmetres que
exigeix `ConstructorsDeVistaTest`.

### Triar la carpeta sense trencar la frontera de capes

El ViewModel no pot conèixer Avalonia, i el selector de carpetes és d'Avalonia. Es fa amb el
mateix mecanisme que la navegació:

```csharp
// CopiaDeSeguretatViewModel
public Interaction<Unit, string?> ShowTriaCarpetaDialog { get; } = new();
```

i el code-behind la resol amb `StorageProvider.OpenFolderPickerAsync`. `NavegacioTest`
només inspecciona les `Interaction` d'`AppStatusViewModel`, de manera que aquesta no li
demana cap comanda de taulell.

### Dos punts fàcils d'equivocar al ViewModel

- **El treball pesat no pot anar al fil d'UI.** `VACUUM INTO` i la compressió són síncrons i
  poden trigar segons: van dins d'un `Task.Run` a l'operació de negoci, i el ViewModel només
  espera.
- **La contrasenya es buida en acabar** (`Password = Confirmacio = string.Empty`), tant si ha
  anat bé com si no.

Validacions al ViewModel (les de negoci són a l'operació i als adaptadors): contrasenya no
buida, mínim 8 caràcters, i les dues iguals.

---

## 11. Missatges d'error que ha de saber donar

Un error de disc o de xarxa no pot arribar a l'usuari com una excepció. La taula mínima:

| Situació | Missatge |
|---|---|
| Cap destí configurat | «Tria on vols desar les còpies abans de fer-ne cap.» |
| La carpeta ja no existeix | «No es troba la carpeta {camí}. Comprova que el llapis o la unitat de xarxa hi siguin.» |
| Carpeta sense permís d'escriptura | «No es pot escriure a {camí}.» |
| Disc ple | «No hi ha prou espai per desar la còpia.» |
| `integrity_check` no diu `ok` | «La còpia de la base de dades no ha passat la comprovació d'integritat; no s'ha desat res.» |
| No es pot escriure al temporal | «No s'ha pogut preparar la còpia: {motiu}.» |
| La retenció falla | La còpia és correcta + avís «No s'han pogut retirar les còpies antigues.» |
| *(Drive)* Sense credencials OAuth | El destí «Google Drive» no surt al selector |
| *(Drive)* L'usuari cancel·la el navegador | «S'ha cancel·lat l'autorització; no s'ha fet cap còpia.» |
| *(Drive)* Compte d'un altre domini | «El compte triat (x@gmail.com) no és del domini xtec.cat.» |
| *(Drive)* Bloquejat pel domini | «L'administrador del domini no permet que EAP Recull accedeixi al Drive. Desa les còpies en una carpeta.» |
| *(Drive)* Sense connexió / Drive ple | «No s'ha pogut connectar amb Google Drive.» / «No hi ha prou espai al Drive del compte.» |

Totes passen abans per `Log.Error`. **`error.log` buit forma part del criteri d'acceptació**
(`ARQUITECTURA.md` §10) per al camí feliç.

---

## 12. Seguretat i protecció de dades

Aquest canvi **modifica la resposta del FAQ del README** («EAP Recull envia alguna dada a
alguna banda? No»). Passa a ser: *no, tret que demanis expressament una còpia de seguretat;
i el que surt de l'ordinador és un zip xifrat que ningú més pot obrir.*

| Risc | Què s'hi fa |
|---|---|
| Dades de menors fora de l'ordinador | El zip es xifra **abans de sortir**, amb AES-256. Al destí —llapis, servidor o núvol— hi arriba text xifrat. La contrasenya no viatja enlloc |
| Núvol d'un compte particular | Amb la carpeta, el destí el tria l'usuari i pot ser institucional. Amb el Drive per API, regla de domini `@xtec.cat` comprovada contra el correu verificat de Google (§8.3) |
| L'aplicació podria tafanejar el Drive | Abast `drive.file`: només veu el que ella mateixa crea. Es diu a la finestra i es veu a la pantalla de consentiment |
| Robatori del fitxer de credencials *(Drive)* | Abast mínim, permisos `600`, fora de la carpeta portable, i «Desconnecta» revoca. **Es desa en clar**: motius més avall |
| Contrasenya feble | Mínim 8 caràcters i confirmació. Res més: una política agressiva acaba en contrasenyes en un post-it |
| Contrasenya perduda | Avís explícit a la finestra i al `LLEGEIX-ME.txt`. **No hi ha recuperació possible**, i és així a posta |
| `.db` en clar al temporal | Carpeta pròpia per execució, esborrada en un `finally` |
| El llapis USB es perd | És el cas per al qual serveix el xifratge. Sense contrasenya, el zip no val res |

**Sobre desar el testimoni de Google en clar** (la pregunta de l'enunciat, «si això és
segur»): és el que fan els clients d'escriptori de Google, i és acceptable *aquí* perquè el
que protegeix el testimoni no és el xifratge sinó **l'abast**: qui l'obtingui pot llegir i
esborrar les còpies xifrades d'EAP Recull, i res més; i les còpies, sense la contrasenya, no
serveixen de res. Alternatives valorades:

- **DPAPI** (`ProtectedData`): transparent per a l'usuari, però **només existeix a Windows** i
  el projecte publica per a quatre plataformes. Millora possible (§15, Q4).
- **Xifrar el testimoni amb la contrasenya de l'usuari**: obligaria a demanar-la també per
  llistar les còpies i trencaria l'objectiu de no reautoritzar cada cop.
- **No desar res**: navegador a cada còpia. És el que l'enunciat vol evitar.

---

## 13. Tests

### Nous, a `BusinessLayer.Integration.Test/CopiaDeSeguretatTest.cs`

Amb `EntornDeTest` (SQLite en memòria, migracions posades) i un `MagatzemFals` sobre una
carpeta temporal. **Cap test toca la xarxa ni obre cap navegador.**

- Amb dades a la base, `Run` acaba bé i el magatzem rep **un** fitxer.
- El fitxer desat és un zip **xifrat**: obrir-lo sense contrasenya falla; amb la contrasenya
  bona, en surt un `BaseDeDades.db` que s'obre i té les mateixes actuacions que l'original.
  *Aquest test val per tots: prova el bolcat, el xifratge i la restauració alhora.*
- Amb la contrasenya equivocada, la lectura del zip falla.
- El nom té el patró `EAPRecull-BaseDeDades-AAAA-MM-DD_hhmm.zip`.
- **Retenció**: amb 3 còpies al destí, la quarta en deixa 3, i la retirada és **la més antiga**.
- **Ordre**: si el desat falla, no es retira res.
- La carpeta temporal queda **buida** en acabar, tant si va bé com si peta.
- Si el magatzem torna `BrokenRules`, `Run` les propaga i no llança.
- Cancel·lació: un token ja cancel·lat acaba amb `BrokenRule`, sense fitxers pel mig.

### Nous, sobre l'adaptador de carpeta (sense base de dades)

- `Llista` **ignora** els fitxers que no segueixen el patró del nom: una carpeta amb altres
  coses a dins no s'ha de tocar mai.
- `RetiraSobrants` no esborra res que no hagi desat ell.
- `Prepara` sobre una carpeta inexistent o de només lectura torna `BrokenRule`, no excepció.
- Escriptura atòmica: durant el desat no hi ha cap `.zip` a mig escriure amb el nom bo.

### Nous, unitaris

- `ZipXifrat`: anada i tornada amb accents i ela geminada als noms d'entrada.
- La regla de domini: `dh@xtec.cat` passa; `dh@gmail.com` i `dh@xtec.cat.attacker.com` no;
  `dh@XTEC.CAT` **ha de passar**.

### Que ja existeixen i s'han de mantenir verds

| Test | Què demana |
|---|---|
| `InjeccioTest.CadaContracteTeLaSevaImplementacioPerConvencio` | `ICopiaDeSeguretat` s'ha de poder **resoldre** en un CI sense xarxa: cap constructor d'adaptador pot tocar res (§9) |
| `NavegacioTest.TotaInteraccioDelTaulellTeLaSevaComanda` | `ShowCopiaDeSeguretatDialog` obliga a `CopiaDeSeguretatCommand` |
| `ConstructorsDeVistaTest` | `CopiaDeSeguretatWindow` amb constructor públic sense paràmetres |
| `RegistreDITest.ElsViewModelsQueLaFactoryHaDeResoldreHiSon` | Cal **afegir-hi** `CopiaDeSeguretatViewModel`: la llista és explícita |
| `BindingsCompilatsTest`, `DissenyTest` | `x:CompileBindings="True"` i cap color literal a l'AXAML nou |

### Proves manuals, un sol cop però imprescindibles

**Fase 1 (carpeta)**: fer una còpia a un llapis → **descarregar-la i obrir-la amb 7-Zip** →
substituir la base de dades i comprovar que el programa arrenca amb ella → fer-ne quatre i
veure que en queden tres → treure el llapis i comprovar el missatge d'error.

**Fase 2 (Drive)**: autoritzar amb un compte `@xtec.cat` → veure el zip al Drive →
descarregar-lo i obrir-lo → provar amb un compte que **no** sigui de `@xtec.cat` i veure que
el rebutja → tancar i reobrir el programa i comprovar que **no** torna a demanar autorització
→ «Desconnecta» i comprovar que sí que la demana.

---

## 14. Documentació a actualitzar

- `README.md`: **el FAQ** («EAP Recull envia alguna dada a alguna banda?»), i una secció nova
  **«Com restaurar una còpia de seguretat»** —el pas que sempre falta i el que de debò salva
  el dia:
  1. Tanca EAP Recull.
  2. Obre el zip amb 7-Zip/Keka amb la teva contrasenya.
  3. Substitueix `BaseDeDades.db` de la carpeta de dades pel del zip.
  4. Torna a obrir EAP Recull.
- `ARQUITECTURA.md`: taula de cicles de vida (§7), taula «Com afegir coses» (§12), i §6 —el
  paràgraf sobre `VACUUM INTO`, que és la manera correcta de copiar aquesta base de dades.
- `agents.md`: el patró «operació de negoci amb port a l'exterior», que és nou.
- `UI.ER.AvaloniaUI/readme.md`: la finestra nova.
- `docs/release_notes.md`.
- El fitxer recordatori `@@ Recorda Fer Copies Periodiques… @@`: ara el programa ja les sap
  fer. Canviar-ne el text perquè apunti al menú (§15, Q6).

---

## 15. Preguntes obertes

| # | Pregunta | Proposta per defecte |
|---|---|---|
| Q1 | **Es fa la prova de §2.1.B abans de res?** | Sí, i abans de tocar cap codi de Google. Costa 15 minuts i decideix si la fase 2 existeix |
| Q2 | Si es fa la fase 2, el domini `@xtec.cat` és **obligatori** o només recomanat? A `IDadesDeLusuari` es va decidir **no** exigir domini, perquè hi ha `@edu.gencat.cat` | Obligatori, com diu l'enunciat, però amb la llista de dominis acceptats en una constant per afegir-hi `edu.gencat.cat` en una línia |
| Q3 | La carpeta per defecte que proposa el programa la primera vegada: cap, o una de calculada? | Cap: que la triï l'usuari. Proposar-ne una convida a acceptar-la sense mirar, i el més probable és que caigui al mateix disc |
| Q4 | Val la pena xifrar el testimoni amb DPAPI a Windows, encara que a Linux/macOS quedi en clar? | No de moment: asimetria per poc guany. S'hi pot afegir després sense tocar res més |
| Q5 | Tres còpies és el número bo, o millor fer-lo configurable? | Tres, i com a constant. Configurable només si algú ho demana |
| Q6 | Es manté el fitxer recordatori de la carpeta de dades? | Sí, canviant-ne el text: «Fes còpies des del menú → Còpia de seguretat» |
| Q7 | Cal avisar al taulell quan fa massa temps de la darrera còpia? La data ja quedarà a l'`Usuari.ini` | Fora d'aquest pla, però és barat: un text a l'`AppStatusViewModel` si han passat més de 30 dies. **És el que fa que les còpies es facin de debò** |

---

## 16. Ordre d'implementació

### Fase 1 — còpies a una carpeta *(no depèn de ningú)*

1. `ZipXifrat` + el seu test. Peça petita, autònoma i verificable en 10 minuts.
2. Els contractes: `IMagatzemDeCopies`, `CopiaRemota`/`Copies`/`DestiDeCopies`, `CopiaResult`,
   `ICopiaDeSeguretat`.
3. `MagatzemFals` als tests i `CopiaDeSeguretat` (bolcat + verificació + zip + desat +
   retenció) contra ell. **A partir d'aquí el nucli és verd i verificat.**
4. `MagatzemDeCarpeta` + els seus tests. Registre a `Injection.cs`.
5. `CopiaDeSeguretatViewModel` + `CopiaDeSeguretatWindow`, entrada de menú, `RegistraNavegacio`
   i el selector de carpeta.
6. Ajustar `RegistreDITest`, bateria sencera, proves manuals, documentació (§14).
   **Aquí ja hi ha còpies de seguretat**, i el que ve després és opcional.

### En paral·lel, quan es pugui

7. La prova de §2.1.B i, si cal, la petició de §2.2 a l'administrador del domini.

### Fase 2 — Google Drive *(només si §2 dona verd)*

8. Client OAuth publicat «en producció» i incrustació de credencials al workflow (§8.2).
9. `MagatzemDrive`: autorització, comprovació de domini, carpeta remota, pujada resumible,
   llistat, retenció, desconnexió. **Una línia** al registre.
10. Selector de destí visible a la finestra i proves manuals de §13.

---

## 17. El que aquest pla no fa

- **No restaura.** Restaurar és substituir un fitxer amb el programa tancat, i un botó que
  sobreescrigui la base de dades viva és molt més perillós que útil. Les instruccions van al
  `LLEGEIX-ME.txt` de dins del zip i al README.
- **No programa còpies automàtiques.** Cap tasca, cap temporitzador, cap còpia en tancar:
  només a petició, com diu l'enunciat.
- **No fa còpies incrementals ni diferencials.** Cada còpia és la base de dades sencera; amb
  aquesta mida, qualsevol altra cosa és complexitat gratuïta.
- **No desa cap altra cosa al destí**: ni informes, ni exportacions pivot, ni el log.
- **No toca l'esquema de la base de dades**: cap migració.
- **No canvia on viu la base de dades** ni com s'hi connecta ningú.

# eaprecull

Gestió d'actuacions EAP. Recull i gestiona les teves actuacions com EAP.

## Estat

* Production Ready:

Per compilar el programa fer:

```bash
(
    # Compilar
    cd UI.ER.AvaloniaUI/;
    dotnet publish -r win-x64 --self-contained --configuration ReleaseComplete -o ../dist; 

    # Crear carpeta per exe
    mkdir -p ../dist-exe; 
    rm -f ../dist-exe/EAPRECULL.zip;

    # Enzipar
    cd ../dist; 
    zip  ../dist-exe/EAPRECULL.zip * ;

    # Esborrar temporals
    cd ..;
    rm -rf dist
)
```

El generarà a la carpeta:

```
./dist-exe
```

## Crear una versió

Les versions es publiquen automàticament amb GitHub Actions
(`.github/workflows/dotnetcore.yml`). El workflow s'engega en fer *push*
d'una etiqueta (*tag*) que comenci per `v20`.

Nomenclatura de les etiquetes: `vAAAA-MM-DD-NNN`, on `NNN` és un
correlatiu de tres dígits dins del mateix dia (p. ex. `v2026-02-12-004`).

Per publicar una versió nova n'hi ha prou amb dues comandes:

```bash
git tag v2026-02-13-001   # canvia la data i el correlatiu
git push --tags           # puja les etiquetes: això dispara el workflow
```

`git push --tags` puja totes les etiquetes locals que encara no siguin al
remot, així no cal repetir el nom de l'etiqueta.

### Encara més fàcil: `git release`

Amb aquest àlies, la data i el correlatiu es calculen sols. Cal
configurar-lo un sol cop:

```bash
git config alias.release '!d=$(date +%F); n=$(git tag -l "v$d-*" | wc -l | tr -d " "); t=$(printf "v%s-%03d" "$d" $((n+1))); git tag -a "$t" -m "$t" && git push origin "$t" && echo "Publicada $t"'
```

I a partir d'aleshores, per publicar:

```bash
git release
```

Que crea, per exemple, `v2026-02-13-001` (o `-002` si avui ja n'hi havia
una) i la puja.

Què fa el workflow:

1. Instal·la el .NET 10 SDK.
2. Compila `UI.ER.AvaloniaUI` amb la configuració `ReleaseComplete`,
   *self-contained*, per a quatre plataformes: `linux-x64`, `win-x64`,
   `osx-arm64` i `osx-x64`.
3. Comprimeix cada publicació en un `.zip`:
   * `EAPRecull-Windows-x64.zip`
   * `EAPRecull-Linux-x64.zip`
   * `EAPRecull-macOS-arm64.zip`
   * `EAPRecull-macOS-x64.zip`
4. Puja els resultats com a *artifacts* de l'execució.
5. Crea la *release* de GitHub amb aquests quatre `.zip` adjunts i les
   notes de la versió generades automàticament a partir dels commits.

No cal cap secret addicional: fa servir el `GITHUB_TOKEN` que
proporciona GitHub Actions.

Si t'equivoques d'etiqueta, esborra-la i torna-hi:

```bash
git tag -d v2026-02-13-001
git push origin :refs/tags/v2026-02-13-001
```

## Objectiu

* Programari lliure per a la gestió de les actuacions del personal de l'EAP

## Contribucions

* Totes les contribucions són benvingudes

## FAQ

* Q: Puc importar dades de l' `EAP Actua` (EapActua)?
* A: Sí, pots exportar la taula d'actuacions a `.\Data\Importacio.xlsx` (worksheet `Data`) i importar-ho amb `ImportData.exe`. Un cop importat has d'informar el nom dels centres (només hi ha els codis) i també cal crear els nivells (infantil, primària, ... )


* Q: Quina llicència té EAP Recull?
* A: MIT. Fes-lo servir sota la teva responsabilitat. EAP recull utilitza llibreries que tenen les seves pròpies llicències, revisa-ho.


* Q: EAP Recull envia alguna dada a alguna banda?
* A: No, pots revisar el codi en aquest repo i compilar la teva versió.


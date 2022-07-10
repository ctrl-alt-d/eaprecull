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
    zip  ../dist-exe/EAPACTUA.zip * ;

    # Esborrar temporals
    cd ..;
    rm -rf dist
)
```

El generarà a la carpeta:

```
./dist-exe
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


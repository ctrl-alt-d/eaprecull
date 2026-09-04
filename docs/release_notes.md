# 0.2

2022.07.02
Actualitzar versions llibreries.
Posar número de versió.

# 0.3

22.07.03
Cerques cas insensitive.

# 0.4

22.07.06
Desactivar alumnes x centre

# 0.5

22.07.06
Filtrar actuacions per alumnes actius

# 0.6

22.07.10
Fer load de references abans de l'update (issue #70)

# 0.7

22.07.10
En comptes de fer load de references abans de l'update (issue #70)
el que es fa és marcar les referències com a modificades (IsModified)

# 0.8

2026.02.08
Migració a net 10

# 0.9
2026.09.04
Refactor Avalonia.UI.
Bus de notificacions.

# 0.10
2026.09.04
Dades de l'usuari (nom, cognoms i adreça xtec) desades a `Usuari.ini`, al costat de la
base de dades.
Nova finestra «Les meves dades», al menú i en obrir el programa si encara no s'han informat.

# 0.11
2026.09.04
Còpies de seguretat de la base de dades, a petició de l'usuari: menú → «Còpia de seguretat».
Bolcat consistent amb `VACUUM INTO`, comprovació d'integritat, zip xifrat amb AES-256 i desat
a una carpeta que tria l'usuari (llapis USB, unitat de xarxa o carpeta local d'un client de
sincronització, que la puja al núvol ja xifrada). Se'n conserven les 3 més recents.
La contrasenya no es desa enlloc.
En obrir el programa, si fa més de dues setmanes de l'última còpia i hi ha actuacions noves,
es proposa fer-ne una. Es pot dir «Ara no». La data i el recompte d'actuacions de l'última
còpia queden a l'`Usuari.ini`.

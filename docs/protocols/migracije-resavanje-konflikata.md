# Migracije: rešavanje konflikata

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Kada vaša grana sadrži migraciju vašeg modula, a u `main` je u međuvremenu stigla druga migracija **istog** modula (od člana vašeg tima). Prepoznaje se po konfliktu na datoteci `<Ime>DbContextModelSnapshot.cs` pri spajanju, ili po tome što nakon spajanja aplikacija pada pri pokretanju primene migracija.

Migracije **različitih** modula ne prave konflikt: svaki modul ima svoj `DbContext`, svoju šemu i svoj snapshot. Ako vidite konflikt sa drugim timom na migracijama, nešto je dublje pogrešno — javite se platformskom timu.

Pošto konflikt nastaje samo unutar tima, najjeftinije rešenje je preventivno: pre nego što krenete u izmenu modela, recite timu. Dve migracije istog modula u paraleli su legitimna situacija, ali najavljena košta pet minuta, a nenajavljena pola dana.

## Zašto ručno spajanje ne dolazi u obzir

Snapshot je generisana datoteka koja mora biti tačan zbir svih migracija. Git ne zna to da garantuje, pa ručno spojen snapshot po pravilu ostavlja EF u stanju u kom sledeća migracija ispada pogrešna — greška koja se ne vidi odmah, nego kod onoga ko sledeći menja model. Protokol zato ne spaja migracije, već vašu migraciju ukloni, primi tuđu iz `main`-a, pa vašu ponovo generiše preko nje.

## Protokol

Sve komande se pokreću iz direktorijuma `backend`; parametri `--project` i `--startup-project` su isti kao u protokolu [Migracije: lokalni rad](migracije-lokalni-rad.md). Koraci su za slučaj da konflikt još niste napravili — to je razlog da se `main` u granu povlači redovno, dok je situacija još u ovom lakšem obliku.

1. **Sklonite svoju migraciju, na svojoj grani, pre spajanja.** Vratite bazu na stanje pre svoje migracije, pa migraciju uklonite:
   ```
   dotnet ef database update PrethodnaMigracija ...
   dotnet ef migrations remove ...
   ```
   Izmene domenskih klasa i EF konfiguracije ostaju — uklanja se samo generisani par migracija/snapshot. Commit-ujte.
2. **Spojite `main` u svoju granu.** Konflikta na migracijama više nema, jer vaša grana sada ne sadrži nijednu svoju migraciju.
3. **Pokrenite aplikaciju**, da se migracija koleg(inic)e iz `main`-a primeni na vašu bazu.
4. **Ponovo dodajte svoju migraciju**, pod istim imenom:
   ```
   dotnet ef migrations add DodataOcenaTure ...
   ```
   EF je sada generiše u odnosu na model koji uključuje obe izmene, i redosled migracija je jednoznačan: tuđa pa vaša.
5. **Pregledajte novu migraciju, pokrenite aplikaciju i testove modula**, pa commit-ujte.

## Ako je konflikt već napravljen

Ako ste spajanje već počeli i git prijavljuje konflikt na snapshot-u, ne dovršavajte ga nagađanjem:

1. Prekinite spajanje: `git merge --abort`.
2. Sada ste nazad na svojoj grani, u stanju pre spajanja — sprovedite protokol iznad od koraka 1.

Ako je pogrešno spojen snapshot već commit-ovan (npr. primetite da `migrations add` generiše besmislice), popravka je ista ideja, samo grublja: obrišite ceo svoj par migracija/snapshot iz radne kopije, vratite `Migrations/` direktorijum na stanje iz `main`-a (`git checkout origin/main -- <putanja do Migrations>`), resetujte lokalnu bazu (`dotnet ef database drop ...`), pa ponovo generišite svoju migraciju. Ako niste sigurni šta je od navedenog vaš slučaj, pozovite platformski tim pre nego što nastavite.

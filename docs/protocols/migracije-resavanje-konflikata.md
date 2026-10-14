# Migracije: rešavanje konflikata

## Kada se primenjuje

Kada vaša grana sadrži migraciju vašeg modula, a u `development` je u međuvremenu stigla druga migracija **istog** modula (od člana vašeg tima). Pouzdano se prepoznaje samo pre spajanja, proverom opisanom u protokolu [Rad sa granama i pull request-ovima](rad-sa-granama.md#dnevna-higijena). Git konflikt na datoteci `<Ime>DbContextModelSnapshot.cs` pri spajanju nije dobar znak jer se javlja samo kada se izmene preklapaju u istom delu snimka modela. Kada dve migracije menjaju različite delove modela, git ih spaja bez konflikta, a greška ostaje skrivena do sledeće izmene modela.

Migracije **različitih** modula ne prave konflikt: svaki modul ima svoj `DbContext`, svoju šemu i svoj snimak modela.

Pošto konflikt nastaje samo unutar tima, najjeftinije rešenje je da javite timu kada ćete menjati model i kada ste završili izmenu. Dve migracije istog modula u paraleli su legitimna situacija, samo je bitno da na kraju sinhronizujete migracije.

## Zašto ručno spajanje ne dolazi u obzir

Snimak modela je generisana datoteka koja mora biti tačan zbir svih migracija. Git ne zna to da garantuje, pa ručno spojen snimak modela po pravilu ostavlja EF u stanju u kom sledeća migracija ispada pogrešna. Protokol zato ne spaja migracije, već vašu migraciju ukloni, primi tuđu iz `development`-a, pa vašu ponovo generiše preko nje.

## Protokol

Sve komande se pokreću iz direktorijuma `backend` i parametri su isti kao u protokolu [Migracije: lokalni rad](migracije-lokalni-rad.md) (`--project`, `--startup-project`, `--context`).

1. **Sklonite svoju migraciju, na svojoj grani, pre spajanja.** Vratite bazu na stanje pre svoje migracije, pa migraciju uklonite:
   ```
   dotnet ef database update PrethodnaMigracija ...
   dotnet ef migrations remove ...
   ```
   Za `PrethodnaMigracija` navedite poslednju migraciju koja je u vašu granu stigla iz `development`-a. Ako grana sadrži više vaših migracija, `migrations remove` pokrenite po jednom za svaku, jer komanda uvek uklanja poslednju.

   Izmene domenskih klasa i EF konfiguracije ostaju, a uklanja se samo generisani par migracija/snimak modela. Commit-ujte.
2. **Spojite `development` u svoju granu.** Konflikta na migracijama više nema, jer vaša grana sada ne sadrži nijednu svoju migraciju.
3. **Pokrenite aplikaciju**, da se migracija kolege iz `development`-a primeni na vašu bazu.
4. **Ponovo dodajte svoju migraciju**, pod istim imenom:
   ```
   dotnet ef migrations add DodataOcenaTure ...
   ```
   EF je sada generiše u odnosu na model koji uključuje obe izmene, i redosled migracija je jednoznačan: tuđa pa vaša. Ako ste u koraku 1 uklonili više migracija, jedna nova migracija obuhvata sve vaše izmene.
5. **Pregledajte novu migraciju, pokrenite aplikaciju i testove modula**, pa commit-ujte.

## Ako je konflikt već napravljen

Ako ste spajanje već počeli i git prijavljuje konflikt na snimku modela, ne dovršavajte ga:

1. Prekinite spajanje: `git merge --abort`.
2. Sada ste nazad na svojoj grani, u stanju pre spajanja — sprovedite protokol iznad od koraka 1.

Ako je pogrešno spojen snimak modela već commit-ovan (npr. primetite da `migrations add` generiše besmislice, ili posao `migrations` odbije pull request nakon što ste `development` već spojili u granu), popravka je slična kao iznad samo zahteva ručno brisanje migracija. Ovo je jedini izuzetak od pravila da se migracije ne brišu ručno. Posle spajanja vaša migracija po vremenu nastanka može da stoji ispred migracije kolege, pa bi `migrations remove` uklonio tuđu migraciju umesto vaše.

1. Iz direktorijuma `Migrations/` obrišite datoteke svojih migracija: za svaku datoteku `<vreme>_<Ime>.cs` i njen par `<vreme>_<Ime>.Designer.cs`.
2. Vratite direktorijum `Migrations/` na stanje iz `development`-a: `git checkout origin/development -- <putanja do Migrations>`. Komanda vraća snimak modela i migracije iz `development`-a, ali ne briše datoteke koje postoje samo u vašoj grani, pa je zato prethodni korak ručan.
3. Resetujte lokalnu bazu (`dotnet ef database drop ...`), jer su na nju primenjene migracije kojih više nema u kodu.
4. Ponovo generišite svoju migraciju, pregledajte je, pokrenite aplikaciju i testove modula, pa commit-ujte.

Ako niste sigurni šta je od navedenog vaš slučaj, pozovite platformski tim pre nego što nastavite.
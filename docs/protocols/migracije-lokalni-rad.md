# Migracije: lokalni rad

## Kada se primenjuje

Svaki put kada menjate model podataka svog modula (menjate agregate ili svojstva njihovih elemenata) ili menjate EF konfiguraciju. Na kraju je pokriven i slučaj kada vam se lokalna baza skroz poremeti.

## Kako sistem radi

Imajte na umu sledeće:

1. **Migracije se primenjuju automatski.** Pri svakom pokretanju aplikacije svaki modul primeni svoje neprimenjene migracije na bazu `explorer` (metoda `MigrateAsync` u inicijalizatoru modula). Zato komandu `dotnet ef database update` nikada ne pokrećete da biste primenili migracije, već samo pokrenete aplikaciju.
2. **Migracija i snimak modela idu u paru.** Komanda za dodavanje migracije generiše datoteku migracije i ažurira `<Ime>DbContextModelSnapshot.cs`. Snimak modela je zbir svih migracija; EF ga koristi da izračuna sledeću migraciju. Zato se migracije nikada ne pišu ni ne brišu ručno. Jedini izuzetak opisuje protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md#ako-je-konflikt-već-napravljen).

Svaki modul ima sopstvenu šemu i sopstvenu tabelu istorije migracija, pa migracije različitih modula ne utiču jedna na drugu.

Sve komande koje navodimo u nastavku pokrećete iz direktorijuma `backend`. Primeri su za modul `Exploration`, a za vaš razvoj koristite naziv svog modula i njegovog konteksta (`<Modul>DbContext`).

## Izmena modela

1. Izmenite domenske klase i, po potrebi, EF konfiguraciju u `Infrastructure`.
2. Dodajte migraciju, sa imenom koje opisuje izmenu:
   ```
   dotnet ef migrations add DodataOcenaTure --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api --context ExplorationDbContext
   ```
3. Pregledajte generisanu datoteku u `Modules/Exploration/Exploration.Infrastructure/Persistence/Migrations/`. Proverite da menja samo ono što ste nameravali i samo šemu vašeg modula. Migracija koja briše kolonu ili tabelu briše i podatke u njoj — to je u razvoju prihvatljivo, ali treba da bude svesno.
4. Pokrenite aplikaciju i migracija se primenjuje automatski. Prođite kroz Scalar slučaj korišćenja koji dira izmenjeni deo modela.
5. Pokrenite testove svog modula. Commit-ujte izmenu modela i migraciju zajedno, kao jednu celinu.

## Vraćanje unazad (rollback)

Primenjuje se kada ste dodali migraciju, pa zaključili da izmena modela nije dobra. Redosled je bitan:

1. Vratite bazu na stanje pre vaše migracije (navodi se ime poslednje migracije koja **ostaje**):
   ```
   dotnet ef database update PrethodnaMigracija --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api --context ExplorationDbContext
   ```
   Spisak migracija, sa oznakom koje su primenjene, daje `dotnet ef migrations list` sa istim parametrima.
2. Uklonite migraciju (briše datoteku migracije i vraća snimak modela):
   ```
   dotnet ef migrations remove --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api --context ExplorationDbContext
   ```
3. Ispravite model, pa dodajte novu migraciju po koracima iznad.

Obrnut redosled (prvo `remove`) ostavlja bazu sa primenjenom migracijom koje više nema u kodu — to stanje aplikacija ne ume sama da razreši i tada sledi reset (ispod).

**Granica:** ovako se vraćaju samo migracije koje još nisu stigle u `development`. Migracija koja je u `development`-u primenjena je kod drugih članova tima; nju ne uklanjate, već dodajete novu migraciju koja menja šta treba. Ako vaša grana i `development` sadrže različite migracije, pratite protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md).

## Zamka pri promeni grane

Kada pređete na granu koja nema migraciju koju ste već primenili (npr. vratite se na `development` sa grane na kojoj ste eksperimentisali), aplikacija neće poništiti izmene u bazi podataka koje je migracija donela. Čest slučaj je pregled tuđeg pull request-a koji sadrži migraciju. Čim pokrenete aplikaciju na toj grani, migracija se primeni na vašu bazu.

Ako se ta migracija kasnije ponovo generiše po protokolu [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md), EF je vidi kao novu migraciju, a vaša baza već sadrži izmene stare verzije. Aplikacija tada pada pri pokretanju (npr. sa greškom da kolona već postoji). Zato posle pregleda grane sa migracijom uradite reset pre nego što nastavite svoj rad.

## Reset lokalne baze

Baza `explorer` je razvojna i sme da se obriše u svakom trenutku. Pri sledećem pokretanju aplikacija je ponovo kreira, primenom svih migracija svih modula. Zaustavite aplikaciju, pa iz direktorijuma `backend` pokrenite:

```
dotnet ef database drop --force --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api --context ExplorationDbContext
```

Parametar `--force` preskače pitanje za potvrdu. Komanda briše celu bazu, a ne samo šemu vašeg modula: nestaju podaci svih modula, uključujući i korisnike koje ste sami registrovali. Posle pokretanja aplikacije baza ponovo sadrži samo početne podatke.

Ovo je standardan izlaz iz svakog nejasnog stanja lokalne baze i nije problem kada se desi.

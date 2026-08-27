# Migracije: lokalni rad

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Svaki put kada menjate model podataka svog modula: dodajete agregat, dodajete ili menjate polje, menjate EF konfiguraciju. Takođe kada vam lokalna baza uđe u stanje koje ne umete da objasnite — na kraju je korak za potpuni reset.

## Kako sistem radi

Dve činjenice objašnjavaju sve korake:

1. **Migracije se primenjuju automatski.** Pri svakom pokretanju aplikacije svaki modul primeni svoje neprimenjene migracije na bazu `explorer` (metoda `MigrateAsync` u inicijalizatoru modula). Zato komandu `dotnet ef database update` nikada ne pokrećete da biste primenili migracije — samo pokrenete aplikaciju. Ručno se baza dira samo pri vraćanju unazad.
2. **Migracija i snapshot idu u paru.** Komanda za dodavanje migracije generiše datoteku migracije i ažurira `<Ime>DbContextModelSnapshot.cs`. Snapshot je zbir svih migracija; EF ga koristi da izračuna sledeću migraciju. Zato se migracije nikada ne pišu ni ne brišu ručno — isključivo kroz `dotnet ef` komande, da snapshot i migracije ostanu usklađeni.

Svaki modul ima sopstvenu šemu i sopstvenu tabelu istorije migracija, pa migracije različitih modula ne utiču jedna na drugu.

## Izmena modela

Sve komande se pokreću iz direktorijuma `backend`. Primeri su za modul `Exploration`; zamenite ime svog modula.

1. Izmenite domenske klase i, po potrebi, EF konfiguraciju u `Infrastructure`.
2. Dodajte migraciju, sa imenom koje opisuje izmenu:
   ```
   dotnet ef migrations add DodataOcenaTure --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api
   ```
3. Pregledajte generisanu datoteku u `Modules/Exploration/Exploration.Infrastructure/Migrations/`. Proverite da menja samo ono što ste nameravali i samo šemu vašeg modula. Migracija koja briše kolonu ili tabelu briše i podatke u njoj — to je u razvoju prihvatljivo, ali treba da bude svesno.
4. Pokrenite aplikaciju — migracija se primenjuje automatski. Prođite kroz Scalar slučaj korišćenja koji dira izmenjeni deo modela.
5. Pokrenite testove svog modula. Commit-ujte izmenu modela i migraciju zajedno, kao jednu celinu.

## Vraćanje unazad (rollback)

Primenjuje se kada ste dodali migraciju, pa zaključili da izmena modela nije dobra. Redosled je bitan: prvo baza, pa migracija.

1. Vratite bazu na stanje pre vaše migracije (navodi se ime poslednje migracije koja **ostaje**):
   ```
   dotnet ef database update PrethodnaMigracija --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api
   ```
   Spisak migracija, sa oznakom koje su primenjene, daje `dotnet ef migrations list` sa istim parametrima.
2. Uklonite migraciju (briše datoteku migracije i vraća snapshot):
   ```
   dotnet ef migrations remove --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api
   ```
3. Ispravite model, pa dodajte novu migraciju po koracima iznad.

Obrnut redosled (prvo `remove`) ostavlja bazu sa primenjenom migracijom koje više nema u kodu — to stanje aplikacija ne ume sama da razreši i tada sledi reset (ispod).

**Granica:** ovako se vraćaju samo migracije koje još nisu stigle u `main`. Migracija koja je u `main`-u primenjena je kod drugih članova tima; nju ne uklanjate, već dodajete novu migraciju koja menja šta treba. Ako vaša grana i `main` sadrže različite migracije, pratite protokol [Migracije: rešavanje konflikata](migracije-resavanje-konflikata.md).

## Zamka pri promeni grane

Kada pređete na granu koja ne sadrži migraciju već primenjenu na vašu bazu (npr. vratite se na `main` sa grane na kojoj ste eksperimentisali), aplikacija to ne vraća unazad — u bazi ostaje višak. Najčešće sve i dalje radi; ako ne radi, ili sumnjate, uradite reset.

## Reset lokalne baze

Baza `explorer` je razvojna i sme da se obriše u svakom trenutku — pri sledećem pokretanju aplikacija je ponovo kreira, primenom svih migracija svih modula:

```
dotnet ef database drop --project Modules/Exploration/Exploration.Infrastructure --startup-project Host.Api
```

Ovo je standardan izlaz iz svakog nejasnog stanja lokalne baze. Nije sramota, brže je od detektivskog posla.

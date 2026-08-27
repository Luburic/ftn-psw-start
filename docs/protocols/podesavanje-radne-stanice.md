# Podešavanje radne stanice

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Jednom, pre prvog rada na projektu. Protokol se završava proverom koja potvrđuje da je radna stanica ispravno podešena — ne prijavljujte da ste spremni dok poslednji korak ne prođe.

## 1. Alati

Instalirajte sledeće, redom kojim su navedeni:

1. **Git** — [git-scm.com](https://git-scm.com/). Podesite ime i adresu koje će stajati uz vaše commit-ove:
   ```
   git config --global user.name "Ime Prezime"
   git config --global user.email "adresa@uns.ac.rs"
   ```
2. **.NET 10 SDK** — [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0). Instalirajte SDK, ne samo runtime. Provera: `dotnet --version` ispisuje verziju koja počinje sa `10.`.
3. **Razvojno okruženje** — Visual Studio 2026 (Community je dovoljan) sa radnim opterećenjem „ASP.NET and web development", ili Rider. Za rad na klijentskoj aplikaciji dovoljan je VS Code.
4. **PostgreSQL 17** — [postgresql.org](https://www.postgresql.org/download/). Instalira se nativno, bez Docker-a. Tokom instalacije:
   - lozinka korisnika `postgres` mora biti `admin`,
   - port ostaje podrazumevani `5432`.

   Ove vrednosti očekuje `appsettings.Development.json` i ne menjaju se lokalno. Provera: pgAdmin (instaliran uz PostgreSQL) uspešno se povezuje na lokalni server.
5. **Node.js LTS** — [nodejs.org](https://nodejs.org/). Provera: `node --version`.
6. **Angular CLI** — `npm install -g @angular/cli`. Provera: `ng version`. (Klijentska aplikacija još nije započeta; alat instalirajte odmah da kasnije ne bi bio uzrok kašnjenja.)

## 2. Projekat

Iz direktorijuma u kom držite projekte:

```
git clone <adresa-repozitorijuma>
cd ftn-psw-start
dotnet tool restore
cd backend
dotnet build
```

`dotnet tool restore` instalira lokalne alate projekta (trenutno `dotnet-ef`, potreban za migracije) prema manifestu u `.config/`. Verzija alata je zajednička za ceo tim — ne instalirajte `dotnet-ef` globalno.

`dotnet build` mora proći bez grešaka. Upozorenja se u ovom projektu tretiraju kao greške, pa je uspešan build jednoznačan signal.

## 3. Prvo pokretanje

Iz direktorijuma `backend`:

```
dotnet run --project Host.Api
```

Pri prvom pokretanju aplikacija sama kreira potrebne strukture u bazi `explorer` — o tome detaljnije u protokolu [Migracije: lokalni rad](migracije-lokalni-rad.md). Zatim u pregledaču otvorite `http://localhost:5000/scalar`: prikazuje se Scalar, interaktivni pregled svih endpoint-a. Kroz njega pozovite `POST /api/identity/register` sa proizvoljnim podacima — odgovor sa JWT tokenom potvrđuje da aplikacija i baza rade zajedno.

## 4. Završna provera

Zaustavite aplikaciju, pa iz direktorijuma `backend` pokrenite:

```
dotnet test
```

Testovi kreiraju sopstvene testne baze i ne diraju bazu `explorer`. Kada svi testovi prođu, radna stanica je spremna.

## Česti problemi

- **`dotnet ef` ne postoji kao komanda** — niste pokrenuli `dotnet tool restore`, ili komandu pokrećete izvan repozitorijuma (alat je lokalan za projekat).
- **Aplikacija pada pri pokretanju uz grešku o konekciji** — PostgreSQL servis nije pokrenut, ili lozinka korisnika `postgres` nije `admin`. Lozinku možete promeniti kroz pgAdmin; ne menjajte `appsettings.Development.json`.
- **Testovi padaju uz grešku o konekciji, a aplikacija radi** — vidite protokol [Testna baza podataka](testna-baza.md).

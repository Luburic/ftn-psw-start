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
5. **Node.js 22 LTS** — [nodejs.org](https://nodejs.org/). Istu verziju koristi CI. Provera: `node --version` ispisuje verziju koja počinje sa `v22.`.
6. **Angular CLI** — `npm install -g @angular/cli`. Provera: `ng --version`.

## 2. Projekat

Iz direktorijuma u kom držite projekte:

```
git clone <adresa-repozitorijuma>
cd ftn-psw-start
dotnet tool restore
cd backend
dotnet build
cd ../frontend
npm install
```

`dotnet tool restore` instalira lokalne alate projekta (trenutno `dotnet-ef`, potreban za migracije) prema manifestu u `.config/`. Verzija alata je zajednička za ceo tim — ne instalirajte `dotnet-ef` globalno.

`dotnet build` mora proći bez grešaka. Upozorenja se u ovom projektu tretiraju kao greške, pa je uspešan build jednoznačan signal.

`npm install` preuzima biblioteke klijentske aplikacije u direktorijum `frontend/node_modules/`, koji se ne commit-uje. Komandu ponavljate posle svakog povlačenja tuđih izmena, jer su one možda donele novu biblioteku.

## 3. Prvo pokretanje

Iz direktorijuma `backend`:

```
dotnet run --project Host.Api
```

Pri prvom pokretanju aplikacija sama kreira potrebne strukture u bazi `explorer` — o tome detaljnije u protokolu [Migracije: lokalni rad](migracije-lokalni-rad.md). Zatim u pregledaču otvorite `http://localhost:5000/scalar`: prikazuje se Scalar, interaktivni pregled svih endpoint-a. Kroz njega pozovite `POST /api/identity/register` sa proizvoljnom e-adresom i lozinkom koja ima bar šest znakova, veliko i malo slovo, cifru i specijalni znak (npr. `Lozinka1!`). Primer koji Scalar sam popuni (`"string"`) ne zadovoljava ta pravila i vraća odgovor 400. Odgovor sa JWT tokenom potvrđuje da aplikacija i baza rade zajedno.

Ne zaustavljajući serversku aplikaciju, u drugom terminalu iz direktorijuma `frontend` pokrenite:

```
npm start
```

U pregledaču otvorite `http://localhost:4200` i prijavite se nalogom koji ste upravo registrovali. Uspešna prijava potvrđuje da klijentska aplikacija stiže do servera — razvojni server zahteve ka `/api` prosleđuje na `localhost:5000` prema datoteci `proxy.conf.json`.

## 4. Završna provera

Zaustavite obe aplikacije, pa iz direktorijuma `backend` pokrenite:

```
dotnet test
```

Testovi kreiraju sopstvene testne baze i ne diraju bazu `explorer`. Zatim iz direktorijuma `frontend` pokrenite iste provere klijenta koje izvršava CI:

```
npm run lint
npm run build
```

Kada svi testovi i obe provere klijenta prođu, radna stanica je spremna.

## Česti problemi

- **`dotnet ef` ne postoji kao komanda** — niste pokrenuli `dotnet tool restore`, ili komandu pokrećete izvan repozitorijuma (alat je lokalan za projekat).
- **Aplikacija pada pri pokretanju uz grešku o konekciji** — PostgreSQL servis nije pokrenut, ili lozinka korisnika `postgres` nije `admin`. Lozinku možete promeniti kroz pgAdmin; ne menjajte `appsettings.Development.json`.
- **Testovi padaju uz grešku o konekciji, a aplikacija radi** — vidite protokol [Testna baza podataka](testna-baza.md).
- **Klijentska aplikacija se učitava, ali prijava i registracija ne rade** — serverska aplikacija nije pokrenuta. Klijent zahteve šalje serveru na `localhost:5000`, pa oba moraju raditi istovremeno.
- **`ng` ili `npm` ne postoji kao komanda** — Node.js nije instaliran ili terminal je otvoren pre instalacije; otvorite nov terminal.

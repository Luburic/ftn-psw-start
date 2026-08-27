# Testna baza podataka

> **Status: normativan.** Koraci u ovom dokumentu odgovaraju stvarnoj konfiguraciji projekta i obavezni su za sve članove tima.

## Kada se primenjuje

Kada integracioni testovi padaju iz razloga koji ne liči na vaš kod: greške o konekciji, o bazi koja ne postoji ili je zauzeta, o tabeli ili koloni koje nema. Za pisanje samih testova merodavan je dokument `docs/knowledge-base/tests/xunit.md`.

## Kako sistem radi

Integracioni testovi ne koriste razvojnu bazu `explorer`. Svaki test projekat kroz `ExplorerApiFactory` dobija sopstvenu bazu po imenu `explorer-test-<modul>` (npr. `explorer-test-exploration`):

- Baza se **obara i ponovo kreira jednom po pokretanju testova**, a migracije primenjuje aplikacija pri podizanju test host-a. Struktura je zato uvek sveža — testna baza ne može da „zastari".
- **Podaci se vraćaju na početno stanje pre svakog testa** (`Reseed`, sa seed podacima modula), pa testovi ne zavise jedni od drugih.
- Konekcija je ista kao razvojna (`localhost:5432`, `postgres`/`admin`); menja se samo ime baze.

Posledica za svakodnevni rad: u testnu bazu se ništa ne dodaje ručno i ništa se iz nje ne čuva. Slobodno je otvorite u pgAdmin-u da pogledate šta je test ostavio, ali računajte da sledeće pokretanje testova briše sve.

## Česti padovi i rešenja

- **`Connection refused` na sve integracione testove** — PostgreSQL servis nije pokrenut. Pokrenite ga (Windows: Services → postgresql), pa ponovo pokrenite testove.
- **`28P01: password authentication failed`** — lozinka korisnika `postgres` nije `admin`. Uskladite lozinku kroz pgAdmin; konekcioni string u kodu se ne menja.
- **Pad pri kreiranju baze, baza „is being accessed by other users"** — obaranje baze prekida i zatečene konekcije (`WITH FORCE`), ali konekciju u kojoj vi držite otvorenu transakciju iz pgAdmin-a ili prethodno zaglavljen test proces ume da nadživi to. Zatvorite pgAdmin konekcije na testnu bazu i proverite da nije ostao zombi `dotnet`/`testhost` proces.
- **`relation ... does not exist` ili nedostaje kolona** — kod i migracije nisu usklađeni: izmenili ste model, a niste dodali migraciju, ili je grana koju ste povukli donela model bez svoje migracije. Vidite protokol [Migracije: lokalni rad](migracije-lokalni-rad.md).
- **Test pada samo kada se pokrene ceo skup, pojedinačno prolazi** — test zavisi od stanja koje je ostavio drugi test, umesto od seed podataka. To je greška u testu: sve što testu treba pravi se u njegovom arrange koraku ili dolazi iz seed-a.

## Druga instanca PostgreSQL-a (izuzetak)

Ako na radnoj stanici već imate PostgreSQL koji ne smete da dirate, testove možete usmeriti na drugu instancu promenljivom okruženja `EXPLORER_TEST_DATABASE` (kompletan konekcioni string; ime baze iz njega dobija sufiks `-<modul>`). Ovo je izuzetak za tu situaciju, ne mehanizam za deljene testne baze — svako testira na sopstvenoj instanci.
